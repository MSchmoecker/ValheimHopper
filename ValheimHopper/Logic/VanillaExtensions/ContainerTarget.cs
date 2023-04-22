using System.Collections.Generic;
using MultiUserChest;
using UnityEngine;
using ValheimHopper.Logic.Helper;

namespace ValheimHopper.Logic {
    public class ContainerTarget : MonoBehaviour, IPushTarget, IPullTarget {
        public HopperPriority PushPriority { get; } = HopperPriority.ContainerPush;
        public HopperPriority PullPriority { get; } = HopperPriority.ContainerPull;
        public bool IsPickup { get; } = false;

        private Container container;

        private void Awake() {
            container = GetComponent<Container>();
        }

        public bool IsValid() {
            return this && container && HopperHelper.IsValidNetView(container.m_nview) && container.m_nview.HasOwner();
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

        public int NetworkHashCode() {
            return HopperHelper.GetNetworkHashCode(container.m_nview);
        }

        public bool Equals(ITarget x, ITarget y) {
            return x == y || x?.NetworkHashCode() == y?.NetworkHashCode();
        }

        public int GetHashCode(ITarget obj) {
            return obj.NetworkHashCode();
        }
    }
}
