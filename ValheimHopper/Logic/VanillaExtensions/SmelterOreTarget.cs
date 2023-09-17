using UnityEngine;
using ValheimHopper.Logic.Helper;

namespace ValheimHopper.Logic {
    public class SmelterOreTarget : NetworkPiece, IPushTarget {
        public HopperPriority PushPriority { get; } = HopperPriority.SmelterOrePush;

        private Smelter smelter;

        protected override void Awake() {
            base.Awake();
            smelter = GetComponent<Smelter>();
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
