namespace LauncherPatcher.Launcher
{
    /// <summary>
    /// The launcher family a file belongs to.
    /// </summary>
    internal enum LauncherKind
    {
        /// <summary>Plain Craft Launcher 2, a .NET assembly.</summary>
        Pcl,

        /// <summary>HMCL, a Java launcher shipped as a .exe with an embedded jar or as a plain .jar.</summary>
        Hmcl,
    }
}
