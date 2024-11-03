using System.Collections.Generic;
using MultiUserChest;
using UnityEngine;
using ValheimHopper.Logic.Helper;

namespace ValheimHopper.Logic {
    public class ContainerTarget : NetworkPiece, IPushTarget, IPullTarget {
        public HopperPriority PushPriority { get; } = HopperPriority.ContainerPush;
        public HopperPriority PullPriority { get; } = HopperPriority.ContainerPull;
        public bool IsPickup { get; } = false;

        private Container container;

        protected override void Awake() {
            base.Awake();
            container = GetComponent<Container>();
        }

        public IEnumerable<ItemDrop.ItemData> GetItems() {
            return container.GetInventory().GetItemInReverseOrder();
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
