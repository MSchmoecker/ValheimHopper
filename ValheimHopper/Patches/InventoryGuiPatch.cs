using HarmonyLib;
using UnityEngine;
using ValheimHopper.Logic;

namespace ValheimHopper.Patches {
    [HarmonyPatch]
    public static class InventoryGuiPatch {
        private static Color ghostColor = new Color(.9f, .9f, .9f, 0.5f);

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateContainer)), HarmonyPostfix]
        public static void UpdateContainerPostfix(InventoryGui __instance) {
            Container container = __instance.m_currentContainer;

            if (container && container.TryGetComponent(out Hopper hopper) && hopper.FilterItemsOption.Get()) {
                ShowContainerGridGhosts(hopper, __instance.m_containerGrid);
            }
        }

        private static void ShowContainerGridGhosts(Hopper hopper, InventoryGrid inventoryGrid) {
            foreach (InventoryGrid.Element element in inventoryGrid.m_elements) {
                if (element.m_icon.enabled) {
                    continue;
                }

                int itemHash = hopper.filter.GetItemHash(element.m_pos);
                GameObject itemGameObject = ObjectDB.instance.GetItemPrefab(itemHash);

                if (!itemGameObject) {
                    continue;
                }

                ItemDrop itemDrop = itemGameObject.GetComponent<ItemDrop>();
                element.m_icon.enabled = true;
                element.m_amount.enabled = true;

                element.m_icon.sprite = itemDrop.m_itemData.GetIcon();
                element.m_icon.color = ghostColor;
                element.m_amount.text = $"0/{itemDrop.m_itemData.m_shared.m_maxStackSize}";
            }
        }
    }
}
