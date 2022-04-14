using System;

namespace ValheimHopper {
    public static class InventoryHelper {
        public static ItemDrop.ItemData FindFirstItem(this Inventory target, Func<ItemDrop.ItemData, bool> predicate) {
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
    }
}
