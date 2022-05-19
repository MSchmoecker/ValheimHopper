using HarmonyLib;

namespace ValheimHopper.Patches {
    [HarmonyPatch]
    public static class PlayerPatch {
        [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost)), HarmonyPostfix]
        public static void UpdatePlacementGhost(Player __instance) {
            if (__instance.m_placementGhost != null) {
                string prefabName = Utils.GetPrefabName(__instance.m_placementGhost.gameObject);

                if (prefabName == "HopperDown" || prefabName == "HopperSide") {
                    __instance.m_placementStatus = Player.PlacementStatus.Valid;
                    __instance.SetPlacementGhostValid(true);
                }
            }
        }
    }
}
