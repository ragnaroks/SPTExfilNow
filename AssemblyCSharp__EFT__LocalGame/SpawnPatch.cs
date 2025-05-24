using SPT.Reflection.Patching;
using System.Reflection;

namespace SPTExfilNow.AssemblyCSharp__EFT__LocalGame {
    public class SpawnPatch : ModulePatch {
        protected override MethodBase GetTargetMethod () {
            return typeof(EFT.LocalGame).GetMethod(nameof(EFT.LocalGame.Spawn), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void Postfix (ref EFT.LocalGame __instance) {
            SPTExfilNowPlugin.LocalGame = __instance;
        }
    }
}
