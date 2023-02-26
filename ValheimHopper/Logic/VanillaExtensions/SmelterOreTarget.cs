using UnityEngine;
using ValheimHopper.Logic.Helper;

namespace ValheimHopper.Logic {
    public class SmelterOreTarget : MonoBehaviour, IPushTarget {
        public HopperPriority PushPriority { get; } = HopperPriority.SmelterOrePush;

        private Smelter smelter;

        private void Awake() {
            smelter = GetComponent<Smelter>();
        }

        public bool IsValid() {
            return this && smelter && HopperHelper.IsValidNetView(smelter.m_nview) && smelter.m_nview.HasOwner();
        }

        public bool CanAddItem(ItemDrop.ItemData item) {
            return smelter.IsItemAllowed(item) && smelter.GetQueueSize() < smelter.m_maxOre;
        }

        public void AddItem(ItemDrop.ItemData item, Inventory source, ZDOID sender) {
            source.RemoveItem(item, 1);
            smelter.m_nview.InvokeRPC("AddOre", item.m_dropPrefab.name);
        }

        public bool InRange(Vector3 position) {
            return HopperHelper.IsInRange(position, smelter.m_addOreSwitch.transform.position, 1f);
        }
    }
}
