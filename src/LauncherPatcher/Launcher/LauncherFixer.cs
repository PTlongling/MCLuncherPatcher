using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using LauncherPatcher.Hmcl;
using LauncherPatcher.Patching;

namespace LauncherPatcher.Launcher
{
    /// <summary>
    /// Applies all patches to a launcher file, and restores it from its backup.
    /// </summary>
    internal static class LauncherFixer
    {
        private const string BackupExtension = ".bak";
        private const string TempExtension = ".pclpp.tmp";

        public static string GetBackupPath(string launcherPath)
        {
            return launcherPath + BackupExtension;
        }

        public static bool HasBackup(string launcherPath)
        {
            return File.Exists(GetBackupPath(launcherPath));
        }

        /// <summary>
        /// Patches the launcher in place. The original file is kept next to it as a backup.
        /// </summary>
        /// <returns>The report of the patched file.</returns>
        public static LauncherReport Fix(string launcherPath)
        {
            byte[] bytes = ReadLauncher(launcherPath);

            ModuleDefMD module = null;
            try
            {
                module = LauncherInspector.LoadModule(bytes);
            }
            catch (BadImageFormatException)
            {
            }

            byte[] patched;
            LauncherReport verified;
            if (module != null)
            {
                EnsureNotRunning(launcherPath, LauncherKind.Pcl);
                using (module)
                    patched = FixPcl(launcherPath, module);
                verified = Verify(launcherPath, patched);
            }
            else
            {
                EnsureNotRunning(launcherPath, LauncherKind.Hmcl);
                patched = FixHmcl(launcherPath, bytes);
                verified = VerifyHmcl(launcherPath, patched);
            }

            WriteLauncher(launcherPath, patched);
            return verified;
        }

        /// <summary>
        /// Replaces the launcher with the backup made by <see cref="Fix"/>.
        /// </summary>
        public static LauncherReport Restore(string launcherPath)
        {
            string backupPath = GetBackupPath(launcherPath);
            if (!File.Exists(backupPath))
                throw new LauncherException("Fix.NoBackup");

            var backup = LauncherInspector.Inspect(backupPath);
            if (backup.Status == LauncherStatus.NotLauncher)
                throw new LauncherException("Fix.BadBackup");

            EnsureNotRunning(launcherPath, backup.Kind);
            try
            {
                File.Copy(backupPath, launcherPath, overwrite: true);
                File.Delete(backupPath);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new LauncherException(ex, "Fix.RestoreFailed", ex.Message);
            }

            return LauncherInspector.Inspect(launcherPath);
        }

        private static byte[] FixPcl(string launcherPath, ModuleDefMD module)
        {
            var report = LauncherInspector.Inspect(launcherPath, module);
            if (!report.CanFix)
                throw new LauncherException(report.MessageKey, report.MessageArgs);

            foreach (var patch in PatchCatalog.All.OfType<Patch>()
                .Where(p => p.GetState(module) == PatchState.Pending))
            {
                try
                {
                    patch.Apply(module);
                }
                catch (PatchException ex)
                {
                    throw new LauncherException(ex, "Fix.PatchFailed", patch.Title, ex.Message);
                }
            }

            AddPatchMarker(module, report.Version);
            return Write(module);
        }

        private static byte[] FixHmcl(string launcherPath, byte[] bytes)
        {
            var jar = JarPackage.Open(bytes);
            if (jar == null)
                throw new LauncherException("Inspect.NotOfficial");

            var report = LauncherInspector.InspectHmcl(launcherPath, bytes, jar);
            if (!report.CanFix)
                throw new LauncherException(report.MessageKey, report.MessageArgs);

            foreach (var patch in HmclPatchCatalog.All.Where(p => p.GetState(jar) == PatchState.Pending))
            {
                try
                {
                    patch.Apply(jar);
                }
                catch (PatchException ex)
                {
                    throw new LauncherException(ex, "Fix.PatchFailed", patch.Title, ex.Message);
                }
            }

            string toolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            jar.AddMarker(toolVersion, report.Version);
            return jar.Save();
        }

        private static byte[] ReadLauncher(string path)
        {
            try
            {
                return File.ReadAllBytes(path);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new LauncherException(ex, "Fix.ReadFailed", ex.Message);
            }
        }

        private static void EnsureNotRunning(string launcherPath, LauncherKind kind)
        {
            if (kind == LauncherKind.Hmcl)
            {
                // HMCL runs under java.exe, so the process list cannot help; an exclusive
                // open fails while the launcher holds the file.
                try
                {
                    using (File.Open(launcherPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                    }
                }
                catch (IOException)
                {
                    throw new LauncherException("Fix.Running");
                }
                return;
            }

            if (LauncherProcess.IsRunning(launcherPath))
                throw new LauncherException("Fix.Running");
        }

        private static void AddPatchMarker(ModuleDef module, string launcherVersion)
        {
            string toolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            string content = $"LauncherPatcher {toolVersion}\nLauncher {launcherVersion}\nPatched {DateTime.UtcNow:O}\n";

            var existing = module.Resources.FindEmbeddedResource(LauncherSignature.PatchMarkerResource);
            if (existing != null)
                module.Resources.Remove(existing);
            module.Resources.Add(new EmbeddedResource(LauncherSignature.PatchMarkerResource,
                Encoding.UTF8.GetBytes(content), ManifestResourceAttributes.Private));
        }

        private static byte[] Write(ModuleDefMD module)
        {
            var options = new ModuleWriterOptions(module)
            {
                Logger = DummyLogger.ThrowModuleWriterExceptionOnErrorInstance,
            };
            options.MetadataOptions.Flags |= MetadataFlags.PreserveAll;

            try
            {
                using (var stream = new MemoryStream())
                {
                    module.Write(stream, options);
                    return stream.ToArray();
                }
            }
            catch (ModuleWriterException ex)
            {
                throw new LauncherException(ex, "Fix.BuildFailed", ex.Message);
            }
        }

        /// <summary>Reloads the patched image and checks every required patch point.</summary>
        private static LauncherReport Verify(string launcherPath, byte[] patched)
        {
            using (var module = LauncherInspector.LoadModule(patched))
            {
                var report = LauncherInspector.Inspect(launcherPath, module);
                // Optional patches may legitimately be absent (not compiled into this build).
                bool complete = report.Status == LauncherStatus.Patched
                    && report.Patches.All(p => p.State == PatchState.Applied
                        || (!p.IsRequired && p.State == PatchState.NotFound));
                if (!complete)
                    throw new LauncherException("Fix.VerifyFailed");
                return report;
            }
        }

        /// <summary>Reopens the patched jar and checks every required patch point.</summary>
        private static LauncherReport VerifyHmcl(string launcherPath, byte[] patched)
        {
            var jar = JarPackage.Open(patched);
            if (jar == null)
                throw new LauncherException("Fix.VerifyFailed");

            var report = LauncherInspector.InspectHmcl(launcherPath, patched, jar);
            bool complete = report.Status == LauncherStatus.Patched
                && report.Patches.All(p => p.State == PatchState.Applied);
            if (!complete)
                throw new LauncherException("Fix.VerifyFailed");
            return report;
        }

        /// <summary>Backs up the original file, then atomically replaces it with the patched bytes.</summary>
        private static void WriteLauncher(string launcherPath, byte[] patched)
        {
            string backupPath = GetBackupPath(launcherPath);
            string tempPath = launcherPath + TempExtension;
            try
            {
                File.Copy(launcherPath, backupPath, overwrite: true);
                File.WriteAllBytes(tempPath, patched);
                File.Copy(tempPath, launcherPath, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new LauncherException(ex, "Fix.WriteFailed", ex.Message);
            }
            finally
            {
                TryDelete(tempPath);
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
