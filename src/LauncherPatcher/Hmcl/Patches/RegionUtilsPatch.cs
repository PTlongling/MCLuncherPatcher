using LauncherPatcher.Localization;

namespace LauncherPatcher.Hmcl.Patches
{
    /// <summary>
    /// Stops HMCL from probing whether the machine and the network are in China mainland,
    /// which decides which download mirrors are used.
    /// </summary>
    internal sealed class RegionUtilsPatch : HmclPatch
    {
        // method bodies are padded with nop (0x00) to their original length;
        // unreachable code is ignored by the JVM verifier, and the stale
        // exception table / StackMapTable are dropped inside the Code attribute.
        private static readonly byte[] RetFalse = { 0x03, 0xAC }; // iconst_0, ireturn
        private static readonly byte[] RetVoid = { 0xB1 };        // return

        protected override string TextKey => "RegionUtils";

        protected override string ClassEntryName => "org/jackhuang/hmcl/util/RegionUtils.class";

        protected override MethodSpec[] Specs { get; } =
        {
            new MethodSpec("init", "()V", RetVoid),
            new MethodSpec("checkMachineIsChinaMainland", "()Z", RetFalse),
            new MethodSpec("isIsChinaMainlandMachine", "()Z", RetFalse),
            new MethodSpec("checkChinaMainlandNetwork", "()Z", RetFalse),
            new MethodSpec("isIsChinaMainlandNetwork", "()Z", RetFalse),
        };

        public override string AppliedText => Text.Get("PatchState.Disabled");
    }
}
