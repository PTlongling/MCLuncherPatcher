using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using LauncherPatcher.Patching;

namespace LauncherPatcher.Hmcl
{
    /// <summary>
    /// An HMCL distribution: a native .exe stub followed by a fat jar, or a plain jar without a stub.
    /// </summary>
    internal sealed class JarPackage
    {
        /// <summary>Main class declared by every HMCL build.</summary>
        public const string MainClassName = "org.jackhuang.hmcl.Main";

        private const string ManifestEntryName = "META-INF/MANIFEST.MF";
        private const string VersionAttribute = "Implementation-Version:";

        private JarPackage(byte[] stub, List<JarEntry> entries)
        {
            Stub = stub;
            Entries = entries;
        }

        /// <summary>The native .exe stub that precedes the jar, or an empty array for a plain jar.</summary>
        public byte[] Stub { get; }

        public List<JarEntry> Entries { get; }

        /// <summary>
        /// Opens an HMCL .exe or .jar. Returns <c>null</c> when the bytes are not an HMCL distribution.
        /// </summary>
        public static JarPackage Open(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length < 4)
                return null;

            if (IsPe(fileBytes))
                return OpenEmbedded(fileBytes);
            return OpenJar(fileBytes, 0);
        }

        /// <summary>The Implementation-Version attribute of the manifest, or <c>null</c> when absent.</summary>
        public string GetVersion()
        {
            string manifest = GetManifest();
            if (manifest == null)
                return null;

            string value = null;
            foreach (string rawLine in manifest.Split('\n'))
            {
                string line = rawLine.TrimEnd('\r');
                // A line starting with a space continues the previous attribute value.
                if (line.StartsWith(" ", StringComparison.Ordinal))
                {
                    if (value != null)
                        value += line.Substring(1);
                    continue;
                }
                if (value != null)
                    return value;
                if (line.StartsWith(VersionAttribute, StringComparison.Ordinal))
                    value = line.Substring(VersionAttribute.Length).Trim();
            }
            return string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>Whether LauncherPatcher's patch marker entry is present.</summary>
        public bool HasMarker()
        {
            return Entries.Any(e => e.Name == LauncherSignature.PatchMarkerResource);
        }

        /// <summary>Records that this jar was patched by LauncherPatcher, replacing any previous marker.</summary>
        public void AddMarker(string toolVersion, string launcherVersion)
        {
            string content = $"LauncherPatcher {toolVersion}\nLauncher {launcherVersion}\nPatched {DateTime.UtcNow:O}\n";
            Entries.RemoveAll(e => e.Name == LauncherSignature.PatchMarkerResource);
            Entries.Add(new JarEntry(LauncherSignature.PatchMarkerResource,
                Encoding.UTF8.GetBytes(content), DateTime.Now));
        }

        /// <summary>Repacks the jar and concatenates the original stub back on top.</summary>
        public byte[] Save()
        {
            byte[] jar;
            using (var stream = new MemoryStream())
            {
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var entry in Entries)
                    {
                        var written = zip.CreateEntry(entry.Name, CompressionLevel.Optimal);
                        written.LastWriteTime = entry.LastWriteTime;
                        using (var entryStream = written.Open())
                            entryStream.Write(entry.Data, 0, entry.Data.Length);
                    }
                }
                jar = stream.ToArray();
            }

            byte[] output = new byte[Stub.Length + jar.Length];
            Array.Copy(Stub, output, Stub.Length);
            Array.Copy(jar, 0, output, Stub.Length, jar.Length);
            return output;
        }

        private static bool IsPe(byte[] data)
        {
            return data.Length > 2 && data[0] == 0x4D && data[1] == 0x5A; // "MZ"
        }

        /// <summary>
        /// Scans the PE overlay for the first "PK\x03\x04" that starts a valid HMCL jar.
        /// </summary>
        private static JarPackage OpenEmbedded(byte[] data)
        {
            for (int i = 0x200; i + 4 <= data.Length; i++)
            {
                if (data[i] != 0x50 || data[i + 1] != 0x4B || data[i + 2] != 0x03 || data[i + 3] != 0x04)
                    continue;

                var package = OpenJar(data, i);
                if (package != null)
                    return package;
            }
            return null;
        }

        private static JarPackage OpenJar(byte[] data, int zipOffset)
        {
            List<JarEntry> entries = ReadEntries(data, zipOffset);
            if (entries == null)
                return null;

            string manifest = null;
            foreach (var entry in entries)
            {
                if (entry.Name == ManifestEntryName)
                    manifest = Encoding.UTF8.GetString(entry.Data);
            }
            if (manifest == null || !manifest.Contains(MainClassName))
                return null;

            byte[] stub = new byte[zipOffset];
            Array.Copy(data, stub, zipOffset);
            return new JarPackage(stub, entries);
        }

        private static List<JarEntry> ReadEntries(byte[] jarBytes, int zipOffset)
        {
            try
            {
                var entries = new List<JarEntry>();
                using (var stream = new MemoryStream(jarBytes, zipOffset, jarBytes.Length - zipOffset))
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    foreach (var entry in zip.Entries)
                    {
                        if (entry.FullName.EndsWith("/", StringComparison.Ordinal))
                            continue; // directory entry
                        byte[] data = new byte[entry.Length];
                        using (var entryStream = entry.Open())
                        {
                            int read = 0;
                            while (read < data.Length)
                            {
                                int n = entryStream.Read(data, read, data.Length - read);
                                if (n <= 0)
                                    throw new IOException("short read on " + entry.FullName);
                                read += n;
                            }
                        }
                        entries.Add(new JarEntry(entry.FullName, data, entry.LastWriteTime.DateTime));
                    }
                }
                return entries;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetManifest()
        {
            foreach (var entry in Entries)
            {
                if (entry.Name == ManifestEntryName)
                    return Encoding.UTF8.GetString(entry.Data);
            }
            return null;
        }
    }
}
