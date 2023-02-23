using HarmonyLib;
using ValheimHopper.Logic;

namespace ValheimHopper.Patches {
    [HarmonyPatch]
    public class ExtensionPatch {
        [HarmonyPatch(typeof(Container), nameof(Container.Awake)), HarmonyPostfix]
        private static void ContainerAwakePostfix(Container __instance) {
            __instance.gameObject.AddComponent<ContainerTarget>();
        }

        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake)), HarmonyPostfix]
        private static void ItemDropAwakePostfix(ItemDrop __instance) {
            __instance.gameObject.AddComponent<ItemDropTarget>();
        }

        [HarmonyPatch(typeof(Smelter), nameof(Smelter.Awake)), HarmonyPostfix]
        private static void SmelterAwakePostfix(Smelter __instance) {
            if (__instance.m_addWoodSwitch) {
                __instance.gameObject.AddComponent<SmelterFuelTarget>();
            }

            if (__instance.m_addOreSwitch) {
                __instance.gameObject.AddComponent<SmelterOreTarget>();
            }
        }
    }
}
