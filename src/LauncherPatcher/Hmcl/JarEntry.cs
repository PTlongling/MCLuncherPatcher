using System;

namespace LauncherPatcher.Hmcl
{
    /// <summary>
    /// One file inside a jar archive.
    /// </summary>
    internal sealed class JarEntry
    {
        public JarEntry(string name, byte[] data, DateTime lastWriteTime)
        {
            Name = name;
            Data = data;
            LastWriteTime = lastWriteTime;
        }

        public string Name { get; }

        public byte[] Data { get; set; }

        public DateTime LastWriteTime { get; }
    }
}
