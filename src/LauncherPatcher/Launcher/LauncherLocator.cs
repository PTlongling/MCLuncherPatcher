using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LauncherPatcher.Hmcl;
using LauncherPatcher.Patching;

namespace LauncherPatcher.Launcher
{
    /// <summary>
    /// Finds a launcher next to LauncherPatcher so the user does not have to pick it manually.
    /// </summary>
    internal static class LauncherLocator
    {
        /// <summary>Upper bound for candidate files; the official launchers are far smaller.</summary>
        private const long MaxCandidateSize = 64L * 1024 * 1024;

        /// <summary>File name prefix of the official HMCL builds.</summary>
        private const string HmclFileNamePrefix = "HMCL";

        public static string FindIn(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            string preferred = Path.Combine(directory, LauncherSignature.DefaultFileName);
            if (File.Exists(preferred))
                return preferred;

            string self = Assembly.GetExecutingAssembly().Location;
            try
            {
                var candidates = new[] { "*.exe", "*.jar" }
                    .SelectMany(pattern => Directory.EnumerateFiles(directory, pattern))
                    .Where(f => !string.Equals(f, self, StringComparison.OrdinalIgnoreCase))
                    .Where(f => new FileInfo(f).Length <= MaxCandidateSize)
                    .ToList();

                string hmcl = candidates.FirstOrDefault(f =>
                    Path.GetFileName(f).StartsWith(HmclFileNamePrefix, StringComparison.OrdinalIgnoreCase));
                if (hmcl != null && IsLauncher(hmcl))
                    return hmcl;

                return candidates.FirstOrDefault(IsLauncher);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return null;
            }
        }

        private static bool IsLauncher(string path)
        {
            return IsPclAssembly(path) || IsHmclPackage(path);
        }

        private static bool IsPclAssembly(string path)
        {
            try
            {
                return AssemblyName.GetAssemblyName(path).Name == LauncherSignature.AssemblyName;
            }
            catch (Exception ex) when (ex is BadImageFormatException || ex is IOException
                || ex is UnauthorizedAccessException || ex is FileLoadException)
            {
                return false;
            }
        }

        private static bool IsHmclPackage(string path)
        {
            try
            {
                return JarPackage.Open(File.ReadAllBytes(path)) != null;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
