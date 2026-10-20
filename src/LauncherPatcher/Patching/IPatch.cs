namespace LauncherPatcher.Patching
{
    /// <summary>
    /// A single modification of a launcher file, independent of the launcher technology.
    /// </summary>
    internal interface IPatch
    {
        /// <summary>Short title shown in the UI.</summary>
        string Title { get; }

        /// <summary>One-sentence explanation shown in the UI.</summary>
        string Description { get; }

        /// <summary>State text shown once the patch has been applied by LauncherPatcher.</summary>
        string AppliedText { get; }

        /// <summary>
        /// Whether this patch is required to restore the download feature.
        /// Launchers that miss a required patch point are reported as unsupported.
        /// </summary>
        bool IsRequired { get; }
    }
}
