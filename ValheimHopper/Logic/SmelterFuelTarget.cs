using UnityEngine;

namespace ValheimHopper.Logic {
    public class SmelterFuelTarget : MonoBehaviour, IPushTarget {
        public HopperPriority PushPriority { get; } = HopperPriority.SmelterFuelPush;

        private Smelter smelter;

        private void Awake() {
            smelter = GetComponent<Smelter>();
        }

        public bool IsValid() {
            return this && smelter && Helper.IsValidNetView(smelter.m_nview) && smelter.m_nview.HasOwner();
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

            smelter.m_nview.InvokeRPC("AddFuel");
        }
        
        public bool InRange(Vector3 position) {
            return Helper.IsInRange(position, smelter.m_addWoodSwitch.transform.position, 1f);
        }
    }
}
