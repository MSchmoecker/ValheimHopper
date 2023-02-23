using System.Collections.Generic;
using UnityEngine;
using ValheimHopper.Logic;

namespace ValheimHopper {
    public class ItemDropTarget : MonoBehaviour, IPullTarget {
        public int PullPriority { get; } = -5;
        public bool IsPickup { get; } = true;

        private ItemDrop itemDrop;

        private void Awake() {
            itemDrop = GetComponent<ItemDrop>();
        }

        public IEnumerable<ItemDrop.ItemData> GetItems() {
            yield return itemDrop.m_itemData;
        }

        public void RemoveItem(ItemDrop.ItemData item, Inventory destination, Vector2i destinationPos, ZDOID sender) {
            if (!itemDrop.m_nview.IsOwner()) {
                itemDrop.RequestOwn();
                return;
            }

            bool wasRemoved = itemDrop.RemoveOne();

            if (!wasRemoved) {
                return;
            }

            ItemDrop.ItemData itemData = itemDrop.m_itemData.Clone();
            destination.AddItem(itemData, 1, destinationPos.x, destinationPos.y);
        }

        public bool IsValid() {
            return this && itemDrop && Helper.IsValidNetView(itemDrop.m_nview);
        }

        public bool InRange(Vector3 position) {
            return true;
        }
    }
}
