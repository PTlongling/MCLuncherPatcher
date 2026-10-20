using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using LauncherPatcher.Hmcl;
using LauncherPatcher.Patching;

namespace LauncherPatcher.Launcher
{
    /// <summary>
    /// Determines whether a file is an official launcher and whether its download feature is disabled.
    /// </summary>
    internal static class LauncherInspector
    {
        public static LauncherReport Inspect(string path)
        {
            if (!File.Exists(path))
                return NotLauncher(path, "Inspect.FileMissing");

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (IOException ex)
            {
                return NotLauncher(path, "Inspect.ReadFailed", ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return NotLauncher(path, "Inspect.AccessDenied");
            }

            ModuleDefMD module;
            try
            {
                module = LoadModule(bytes);
            }
            catch (BadImageFormatException)
            {
                module = null;
            }

            if (module != null)
            {
                using (module)
                    return Inspect(path, module);
            }

            var jar = JarPackage.Open(bytes);
            if (jar == null)
                return NotLauncher(path, "Inspect.NotOfficial");
            return InspectHmcl(path, bytes, jar);
        }

        public static LauncherReport Inspect(string path, ModuleDefMD module)
        {
            if (module.Assembly == null || module.Assembly.Name != LauncherSignature.AssemblyName)
                return NotLauncher(path, "Inspect.NotOfficial");

            string version = module.Assembly.Version.ToString();
            var patches = PatchCatalog.All.OfType<Patch>()
                .Select(p => new PatchReport(p, p.GetState(module))).ToList();
            bool hasMarker = module.Resources.Any(r => r.Name == LauncherSignature.PatchMarkerResource);

            var status = DeriveStatus(patches, hasMarker, out string messageKey);
            return new LauncherReport(path, LauncherKind.Pcl, version, status, patches, messageKey);
        }

        /// <summary>Inspects an already opened HMCL distribution, without touching the disk again.</summary>
        internal static LauncherReport InspectHmcl(string path, byte[] bytes, JarPackage jar)
        {
            string version = jar.GetVersion();
            var patches = HmclPatchCatalog.All.Select(p => new PatchReport(p, p.GetState(jar))).ToList();

            var status = DeriveStatus(patches, jar.HasMarker(), out string messageKey);
            return new LauncherReport(path, LauncherKind.Hmcl, version, status, patches, messageKey);
        }

        internal static ModuleDefMD LoadModule(byte[] data)
        {
            return ModuleDefMD.Load(data, new ModuleCreationOptions { TryToLoadPdbFromDisk = false });
        }

        // Older releases differ in layout, so a patch point that is missing only matters when something is disabled.
        private static LauncherStatus DeriveStatus(List<PatchReport> patches, bool hasMarker, out string messageKey)
        {
            var required = patches.Where(p => p.IsRequired).ToList();
            if (required.All(p => p.State != PatchState.Pending))
            {
                messageKey = hasMarker ? "Inspect.Patched" : "Inspect.NotBlocked";
                return hasMarker ? LauncherStatus.Patched : LauncherStatus.NotBlocked;
            }
            if (required.All(p => p.State != PatchState.NotFound))
            {
                messageKey = "Inspect.Blocked";
                return LauncherStatus.Blocked;
            }
            messageKey = "Inspect.Unsupported";
            return LauncherStatus.Unsupported;
        }

        private static LauncherReport NotLauncher(string path, string messageKey, params object[] args)
        {
            return new LauncherReport(path, LauncherKind.Pcl, null, LauncherStatus.NotLauncher,
                Array.Empty<PatchReport>(), messageKey, args);
        }
    }
}
