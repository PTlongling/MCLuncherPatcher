using System.Collections.Generic;
using LauncherPatcher.Hmcl.Patches;

namespace LauncherPatcher.Hmcl
{
    /// <summary>
    /// All patches LauncherPatcher applies to HMCL, in application order.
    /// </summary>
    internal static class HmclPatchCatalog
    {
        public static IReadOnlyList<HmclPatch> All { get; } = new HmclPatch[]
        {
            new RegionUtilsPatch(),
            new IntegrityCheckerPatch(),
        };
    }
}
