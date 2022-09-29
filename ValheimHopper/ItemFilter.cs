using System.Collections.Generic;

namespace ValheimHopper {
    public class ItemFilter {
        private ZNetView zNetView;
        private Inventory inventory;

        private Dictionary<string, ZInt> slots = new Dictionary<string, ZInt>();

        public ItemFilter(ZNetView zNetView, Inventory inventory) {
            this.zNetView = zNetView;
            this.inventory = inventory;
        }

        public void Save() {
            foreach (ItemDrop.ItemData item in inventory.m_inventory) {
                Vector2i gridPos = item.m_gridPos;
                int itemHash = item.m_dropPrefab.name.GetStableHashCode();
                SetItemHash(gridPos, itemHash);
            }
        }

        public void Clear() {
            for (int x = 0; x < inventory.m_width; x++) {
                for (int y = 0; y < inventory.m_height; y++) {
                    SetItemHash(x, y, 0);
                }
            }
        }

        public void Copy(ItemFilter other) {
            for (int x = 0; x < inventory.m_width; x++) {
                for (int y = 0; y < inventory.m_height; y++) {
                    SetItemHash(x, y, other.GetItemHash(x, y));
                }
            }
        }

        public void SetItemHash(Vector2i gridPos, int itemHash) {
            SetItemHash(gridPos.x, gridPos.y, itemHash);
        }

        public int GetItemHash(Vector2i gridPos) {
            return GetItemHash(gridPos.x, gridPos.y);
        }

        public void SetItemHash(int x, int y, int itemHash) {
            GetSlot(x, y).Set(itemHash);
        }

        public int GetItemHash(int x, int y) {
            return GetSlot(x, y).Get();
        }

        private ZInt GetSlot(int x, int y) {
            string key = $"hopper_filter_{x}_{y}";

            if (slots.TryGetValue(key, out ZInt slot)) {
                return slot;
            }

            slot = new ZInt(key, 0, zNetView);
            slots[key] = slot;
            return slot;
        }
    }
}
