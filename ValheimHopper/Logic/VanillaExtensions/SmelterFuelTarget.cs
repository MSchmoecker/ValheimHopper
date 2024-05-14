using UnityEngine;
using ValheimHopper.Logic.Helper;

namespace ValheimHopper.Logic {
    public class SmelterFuelTarget : NetworkPiece, IPushTarget {
        public HopperPriority PushPriority { get; } = HopperPriority.SmelterFuelPush;

        private Smelter smelter;

        protected override void Awake() {
            base.Awake();
            smelter = GetComponent<Smelter>();
        }

        public bool CanAddItem(ItemDrop.ItemData item) {
            bool isFuelItem = smelter.m_fuelItem && smelter.m_fuelItem.m_itemData.m_shared.m_name == item.m_shared.m_name;
            return isFuelItem && smelter.GetFuel() < smelter.m_maxFuel - 1;
        }

        public void AddItem(ItemDrop.ItemData item, Inventory source, ZDOID sender) {
            bool removed = source.RemoveItem(item, 1);

            if (!removed) {
                return;
            }

            smelter.m_nview.InvokeRPC("RPC_AddFuel");
        }

        public bool InRange(Vector3 position) {
            return HopperHelper.IsInRange(position, smelter.m_addWoodSwitch.transform.position, 1f);
        }
    }
}
