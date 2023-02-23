using UnityEngine;

namespace ValheimHopper.Logic {
    public class SmelterOreTarget : MonoBehaviour, IPushTarget {
        public int PushPriority { get; } = 5;

        private Smelter smelter;

        private void Awake() {
            smelter = GetComponent<Smelter>();
        }

        public bool IsValid() {
            return this && smelter && Helper.IsValidNetView(smelter.m_nview) && smelter.m_nview.HasOwner();
        }

        public bool CanAddItem(ItemDrop.ItemData item) {
            return smelter.IsItemAllowed(item) && smelter.GetQueueSize() < smelter.m_maxOre;
        }

        public void AddItem(ItemDrop.ItemData item, Inventory source, ZDOID sender) {
            source.RemoveItem(item, 1);
            smelter.m_nview.InvokeRPC("AddOre", item.m_dropPrefab.name);
        }

        public bool InRange(Vector3 position) {
            return Helper.IsInRange(position, smelter.m_addOreSwitch.transform.position, 1f);
        }
    }
}
