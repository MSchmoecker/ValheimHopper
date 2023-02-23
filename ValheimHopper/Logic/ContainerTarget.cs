using System;
using System.Collections.Generic;
using MultiUserChest;
using UnityEngine;

namespace ValheimHopper.Logic {
    public class ContainerTarget : MonoBehaviour, IPushTarget, IPullTarget {
        public int PushPriority { get; } = 10;
        public int PullPriority { get; } = 10;
        public bool IsPickup { get; } = false;

        private Container container;

        private void Awake() {
            container = GetComponent<Container>();
        }

        public bool IsValid() {
            return this && container && Helper.IsValidNetView(container.m_nview) && container.m_nview.HasOwner();
        }

        public IEnumerable<ItemDrop.ItemData> GetItems() {
            return container.GetInventory().GetItemInOrder();
        }

        public void AddItem(ItemDrop.ItemData item, Inventory source, ZDOID sender) {
            container.AddItemToChest(item, source, new Vector2i(-1, -1), sender, 1);
        }

        public void RemoveItem(ItemDrop.ItemData item, Inventory destination, Vector2i destinationPos, ZDOID sender) {
            container.RemoveItemFromChest(item, destination, destinationPos, sender, 1);
        }

        public bool CanAddItem(ItemDrop.ItemData item) {
            return container.GetInventory().CanAddItem(item, 1);
        }

        public bool InRange(Vector3 position) {
            return true;
        }
    }
}
