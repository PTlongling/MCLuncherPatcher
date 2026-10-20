using System.Linq;
using LauncherPatcher.Localization;
using LauncherPatcher.Patching;

namespace LauncherPatcher.Hmcl
{
    /// <summary>
    /// A single modification of the methods of one class inside an HMCL jar.
    /// </summary>
    internal abstract class HmclPatch : IPatch
    {
        /// <summary>Identifier used for the localized title and description ("Patch.{TextKey}.Title").</summary>
        protected abstract string TextKey { get; }

        /// <summary>Jar entry name of the class this patch modifies.</summary>
        protected abstract string ClassEntryName { get; }

        /// <summary>The methods to rewrite, in the order they must be matched.</summary>
        protected abstract MethodSpec[] Specs { get; }

        /// <summary>Short title shown in the UI.</summary>
        public string Title => Text.Get($"Patch.{TextKey}.Title");

        /// <summary>One-sentence explanation shown in the UI.</summary>
        public string Description => Text.Get($"Patch.{TextKey}.Description");

        /// <summary>State text shown once the patch has been applied by LauncherPatcher.</summary>
        public virtual string AppliedText => Text.Get("PatchState.Restored");

        /// <summary>
        /// Whether this patch is required to restore the download feature.
        /// Launchers that miss a required patch point are reported as unsupported.
        /// </summary>
        public virtual bool IsRequired => true;

        public PatchState GetState(JarPackage jar)
        {
            var entry = FindEntry(jar);
            if (entry == null)
                return PatchState.NotFound;

            bool pending = false;
            foreach (var spec in Specs)
            {
                byte[] code = ClassPatcher.GetMethodCode(entry.Data, spec.Name, spec.Desc);
                if (code == null)
                    return PatchState.NotFound;
                if (!code.SequenceEqual(spec.Code))
                    pending = true;
            }
            return pending ? PatchState.Pending : PatchState.Applied;
        }

        public void Apply(JarPackage jar)
        {
            var entry = FindEntry(jar)
                ?? throw new PatchException($"patch point of {TextKey} not found");
            if (GetState(jar) != PatchState.Pending)
                throw new PatchException($"{TextKey} is not pending");

            entry.Data = ClassPatcher.PatchClass(entry.Data, Specs);
        }

        private JarEntry FindEntry(JarPackage jar)
        {
            return jar.Entries.FirstOrDefault(e => e.Name == ClassEntryName);
        }
    }
}
