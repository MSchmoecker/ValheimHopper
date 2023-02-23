namespace ValheimHopper.Logic {
    public interface IPushTarget : ITarget {
        int PushPriority { get; }

        bool CanAddItem(ItemDrop.ItemData item);
        void AddItem(ItemDrop.ItemData item, Inventory source, ZDOID sender);
    }
}
