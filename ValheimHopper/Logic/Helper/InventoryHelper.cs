using System;
using System.Collections.Generic;

namespace ValheimHopper.Logic.Helper {
    public static class InventoryHelper {
        public static ItemDrop.ItemData FindLastItem(this Inventory target, Func<ItemDrop.ItemData, bool> predicate) {
            if (target.m_inventory.Count == 0) {
                return null;
            }

            for (int y = target.m_height - 1; y >= 0; y--) {
                for (int x = target.m_width - 1; x >= 0; x--) {
                    ItemDrop.ItemData item = target.GetItemAt(x, y);

                    if (item != null && predicate(item)) {
                        return item;
                    }
                }
            }

            return null;
        }

        public static IEnumerable<ItemDrop.ItemData> GetItemInReverseOrder(this Inventory target) {
            if (target.m_inventory.Count == 0) {
                yield break;
            }

            for (int y = target.m_height - 1; y >= 0; y--) {
                for (int x = target.m_width - 1; x >= 0; x--) {
                    ItemDrop.ItemData item = target.GetItemAt(x, y);

                    if (item != null) {
                        yield return item;
                    }
                }
            }
        }
    }
}
