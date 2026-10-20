using System.Collections.Generic;
using LauncherPatcher.Patching.Patches;

namespace LauncherPatcher.Patching
{
    /// <summary>
    /// All patches LauncherPatcher applies, in application order.
    /// </summary>
    internal static class PatchCatalog
    {
        public static IReadOnlyList<IPatch> All { get; } = new Patch[]
        {
            new VersionListPatch(),
            new ModpackPickerPatch(),
            new ModpackDispatchPatch(),
            new CardVisibilityPatch(),
            new RestrictConstantPatch(),
            new UpdateBlockPatch(),
        };
    }
}
