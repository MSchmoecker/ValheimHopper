using HarmonyLib;
using Jotunn.Managers;
using MultiUserChest;
using UnityEngine;
using UnityEngine.UI;

namespace ValheimHopper.Patches {
    [HarmonyPatch]
    public static class InventoryGUIPatch {
        private static GameObject toggleGameObject;
        private static Toggle toggle;

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake)), HarmonyPostfix]
        public static void InventoryGuiAwakePatch(InventoryGui __instance) {
            RectTransform container = __instance.m_container;

            toggleGameObject = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("Toggle"), container);
            toggle = toggleGameObject.GetComponent<Toggle>();
            Text text = toggle.GetComponentInChildren<Text>();

            GUIManager.Instance.ApplyToogleStyle(toggle);
            GUIManager.Instance.ApplyTextStyle(text, GUIManager.Instance.ValheimOrange, 18);

            text.text = Localization.instance.Localize("$toggle_leave_one_item");
            toggle.onValueChanged.AddListener(on => {
                if (__instance.m_currentContainer && __instance.m_currentContainer.TryGetComponent(out Hopper hopper)) {
                    hopper.SetLeaveOneItem(on);
                }
            });
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update)), HarmonyPostfix]
        public static void InventoryGuiUpdatePatch(InventoryGui __instance) {
            if (__instance.m_currentContainer && __instance.m_currentContainer.TryGetComponent(out Hopper hopper)) {
                toggleGameObject.SetActive(true);
                toggle.isOn = hopper.zNetView.GetZDO().GetBool("hopper_leave_one_item");
            } else {
                toggleGameObject.SetActive(false);
            }
        }
    }
}
