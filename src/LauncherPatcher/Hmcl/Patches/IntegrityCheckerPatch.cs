using LauncherPatcher.Localization;

namespace LauncherPatcher.Hmcl.Patches
{
    /// <summary>
    /// Skips HMCL's self integrity check, equivalent to always running with
    /// <c>-Dhmcl.self_integrity_check.disable=true</c>.
    /// </summary>
    internal sealed class IntegrityCheckerPatch : HmclPatch
    {
        private static readonly byte[] RetTrue = { 0x04, 0xAC }; // iconst_1, ireturn
        private static readonly byte[] RetVoid = { 0xB1 };       // return

        protected override string TextKey => "IntegrityChecker";

        protected override string ClassEntryName => "org/jackhuang/hmcl/upgrade/IntegrityChecker.class";

        protected override MethodSpec[] Specs { get; } =
        {
            new MethodSpec("verifyJar", "(Ljava/nio/file/Path;)V", RetVoid),
            new MethodSpec("isSelfVerified", "()Z", RetTrue),
        };

        public override string AppliedText => Text.Get("PatchState.Disabled");
    }
}
