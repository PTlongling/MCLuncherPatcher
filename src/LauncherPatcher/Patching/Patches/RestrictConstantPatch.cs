using dnlib.DotNet;

namespace LauncherPatcher.Patching.Patches
{
    /// <summary>
    /// Flips the <c>ModSecret.RestrictMcDownload</c> constant from <c>true</c> to <c>false</c>,
    /// lifting the master switch that disables the download feature.
    /// </summary>
    /// <remarks>
    /// The patch targets a field constant instead of a method, so it overrides the
    /// module-level entry points and leaves the <see cref="MethodDef"/> template unused.
    /// </remarks>
    internal sealed class RestrictConstantPatch : Patch
    {
        private const string SecretType = "PCL.ModSecret";
        private const string RestrictField = "RestrictMcDownload";

        protected override string TextKey => "RestrictConstant";

        public override bool IsRequired => false;

        public override PatchState GetState(ModuleDef module)
        {
            var constant = FindField(module)?.Constant;
            if (constant?.Value is bool restricted)
                return restricted ? PatchState.Pending : PatchState.Applied;
            return PatchState.NotFound;
        }

        public override void Apply(ModuleDef module)
        {
            var field = FindField(module)
                ?? throw new PatchException($"patch point of {TextKey} not found");
            if (GetState(module) != PatchState.Pending)
                throw new PatchException($"{TextKey} is not pending");

            field.Constant.Value = false;
        }

        protected override MethodDef FindTarget(ModuleDef module)
        {
            return null;
        }

        protected override PatchState GetState(ModuleDef module, MethodDef target)
        {
            return PatchState.NotFound;
        }

        protected override void Apply(ModuleDef module, MethodDef target)
        {
        }

        private static FieldDef FindField(ModuleDef module)
        {
            return ModuleQuery.FindType(module, SecretType)?.FindField(RestrictField);
        }
    }
}
