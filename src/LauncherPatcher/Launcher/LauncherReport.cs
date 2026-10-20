using System.Collections.Generic;
using LauncherPatcher.Localization;
using LauncherPatcher.Patching;

namespace LauncherPatcher.Launcher
{
    /// <summary>
    /// Result of inspecting a launcher file.
    /// </summary>
    internal sealed class LauncherReport
    {
        public LauncherReport(string path, LauncherKind kind, string version, LauncherStatus status,
            IReadOnlyList<PatchReport> patches, string messageKey, params object[] messageArgs)
        {
            Path = path;
            Kind = kind;
            Version = version;
            Status = status;
            Patches = patches;
            MessageKey = messageKey;
            MessageArgs = messageArgs;
        }

        public string Path { get; }

        /// <summary>The launcher family this file belongs to.</summary>
        public LauncherKind Kind { get; }

        /// <summary>Launcher version, or <c>null</c> when the file is not a launcher.</summary>
        public string Version { get; }

        public LauncherStatus Status { get; }

        public IReadOnlyList<PatchReport> Patches { get; }

        public bool CanFix => Status == LauncherStatus.Blocked;

        /// <summary>User-facing explanation of <see cref="Status"/>, in the current display language.</summary>
        public string Message => Text.Format(MessageKey, MessageArgs);

        internal string MessageKey { get; }

        internal object[] MessageArgs { get; }
    }

    /// <summary>
    /// State of one patch inside a <see cref="LauncherReport"/>. Texts follow the current display language.
    /// </summary>
    internal sealed class PatchReport
    {
        private readonly IPatch _patch;

        public PatchReport(IPatch patch, PatchState state)
        {
            _patch = patch;
            State = state;
        }

        public string Title => _patch.Title;

        public string Description => _patch.Description;

        public string AppliedText => _patch.AppliedText;

        public bool IsRequired => _patch.IsRequired;

        public PatchState State { get; }
    }
}
