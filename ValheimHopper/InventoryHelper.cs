using System;
using System.Collections.Generic;

namespace ValheimHopper {
    public static class InventoryHelper {
        public static ItemDrop.ItemData FindFirstItem(this Inventory target, Func<ItemDrop.ItemData, bool> predicate) {
            if (target.m_inventory.Count == 0) {
                return null;
            }

            for (int y = 0; y < target.m_height; y++) {
                for (int x = 0; x < target.m_width; x++) {
                    ItemDrop.ItemData item = target.GetItemAt(x, y);

                    if (item != null && predicate(item)) {
                        return item;
                    }
                }
            }

            return null;
        }

        public static IEnumerable<ItemDrop.ItemData> GetItemInOrder(this Inventory target) {
            if (target.m_inventory.Count == 0) {
                yield break;
            }

            for (int y = 0; y < target.m_height; y++) {
                for (int x = 0; x < target.m_width; x++) {
                    ItemDrop.ItemData item = target.GetItemAt(x, y);

                    if (item != null) {
                        yield return item;
                    }
                }
            }
        }
    }
}
