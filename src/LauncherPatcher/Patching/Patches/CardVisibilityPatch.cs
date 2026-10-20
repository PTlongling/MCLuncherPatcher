using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace LauncherPatcher.Patching.Patches
{
    /// <summary>
    /// Makes the hidden Minecraft card on the "Download" page visible again.
    /// </summary>
    /// <remarks>
    /// <c>PageDownloadModify</c> hides the card with <c>CardMinecraft.Visibility = Collapsed</c>.
    /// The patch rewrites the constant to <c>Visibility.Visible</c> at every assignment site.
    /// Several methods can assign the property, so the patch overrides the module-level entry
    /// points and leaves the <see cref="MethodDef"/> template unused.
    /// </remarks>
    internal sealed class CardVisibilityPatch : Patch
    {
        private const string PageType = "PCL.PageDownloadModify";
        private const string CardGetter = "get_CardMinecraft";
        private const string VisibilitySetter = "set_Visibility";
        private const string VisibilityType = "System.Windows.UIElement";

        protected override string TextKey => "CardVisibility";

        public override bool IsRequired => false;

        public override PatchState GetState(ModuleDef module)
        {
            var type = ModuleQuery.FindType(module, PageType);
            if (type == null)
                return PatchState.NotFound;

            bool collapsed = false;
            bool visible = false;
            foreach (var site in FindSites(type))
            {
                if (site.OpCode.Code == Code.Ldc_I4_2)
                    collapsed = true;
                else if (site.OpCode.Code == Code.Ldc_I4_0)
                    visible = true;
            }

            if (collapsed)
                return PatchState.Pending;
            return visible ? PatchState.Applied : PatchState.NotFound;
        }

        public override void Apply(ModuleDef module)
        {
            var type = ModuleQuery.FindType(module, PageType)
                ?? throw new PatchException($"patch point of {TextKey} not found");

            int applied = 0;
            foreach (var site in FindSites(type))
            {
                if (site.OpCode.Code != Code.Ldc_I4_2)
                    continue;
                site.OpCode = OpCodes.Ldc_I4_0;
                site.Operand = null;
                applied++;
            }
            if (applied == 0)
                throw new PatchException($"{TextKey} is not pending");
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

        /// <summary>
        /// Returns the ldc instruction of every <c>CardMinecraft.Visibility = …</c> assignment:
        /// call get_CardMinecraft → ldc.i4 → call set_Visibility.
        /// </summary>
        private static System.Collections.Generic.IEnumerable<Instruction> FindSites(TypeDef type)
        {
            foreach (var method in type.Methods.Where(m => m.HasBody))
            {
                var instructions = method.Body.Instructions;
                for (int i = 0; i + 2 < instructions.Count; i++)
                {
                    if (!ModuleQuery.IsCallTo(instructions[i], PageType, CardGetter))
                        continue;
                    if (!ModuleQuery.IsCallTo(instructions[i + 2], VisibilityType, VisibilitySetter))
                        continue;
                    if (instructions[i + 1].OpCode.Code == Code.Ldc_I4_2
                        || instructions[i + 1].OpCode.Code == Code.Ldc_I4_0)
                        yield return instructions[i + 1];
                }
            }
        }
    }
}
