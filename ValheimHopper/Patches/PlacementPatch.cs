using HarmonyLib;
using ValheimHopper.Logic;

namespace ValheimHopper.Patches {
    [HarmonyPatch]
    public class PlacementPatch {
        [HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost)), HarmonyPostfix]
        public static void SetupPlacementGhostPatch(Player __instance) {
            if (!__instance.m_placementGhost) {
                return;
            }

            foreach (Hopper hopper in __instance.m_placementGhost.GetComponentsInChildren<Hopper>()) {
                UnityEngine.Object.Destroy(hopper);
            }

            foreach (Pipe hopper in __instance.m_placementGhost.GetComponentsInChildren<Pipe>()) {
                UnityEngine.Object.Destroy(hopper);
            }
        }
    }
}
