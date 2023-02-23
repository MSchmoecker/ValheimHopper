using System.Collections;
using System.Collections.Generic;

namespace ValheimHopper.Logic {
    public interface IPullTarget : ITarget {
        int PullPriority { get; }
        bool IsPickup { get; }

        IEnumerable<ItemDrop.ItemData> GetItems();
        void RemoveItem(ItemDrop.ItemData item, Inventory destination, Vector2i destinationPos, ZDOID sender);
    }
}
