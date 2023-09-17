using System.Collections.Generic;
using UnityEngine;
using ValheimHopper.Logic.Helper;

namespace ValheimHopper.Logic {
    public class BeehiveTarget : NetworkPiece, IPullTarget {
        public HopperPriority PullPriority { get; } = HopperPriority.BeehivePull;
        public bool IsPickup { get; } = false;

        private Beehive beehive;
        private const string RequestOwnershipRPC = "VH_RequestOwnership";

        protected override void Awake() {
            base.Awake();

            beehive = GetComponent<Beehive>();
            beehive.m_nview.Register(RequestOwnershipRPC, RPC_RequestOwnership);
        }

        public bool InRange(Vector3 position) {
            return true;
        }

        public IEnumerable<ItemDrop.ItemData> GetItems() {
            if (beehive.m_honeyItem && beehive.GetHoneyLevel() > 0) {
                ItemHelper.CheckDropPrefab(beehive.m_honeyItem);
                yield return beehive.m_honeyItem.m_itemData;
            }
        }

        public void RemoveItem(ItemDrop.ItemData item, Inventory destination, Vector2i destinationPos, ZDOID sender) {
            if (!beehive.m_nview.IsOwner()) {
                beehive.m_nview.InvokeRPC(RequestOwnershipRPC);
                return;
            }

            int honeyLevel = beehive.GetHoneyLevel();
            if (honeyLevel <= 0) {
                return;
            }

            beehive.m_nview.GetZDO().Set("level", honeyLevel - 1);
            destination.AddItem(item, 1, destinationPos.x, destinationPos.y);
        }

        private void RPC_RequestOwnership(long sender) {
            if (!beehive.m_nview.IsOwner()) {
                return;
            }

            beehive.m_nview.GetZDO().SetOwner(sender);
            ZDOMan.instance.ForceSendZDO(sender, beehive.m_nview.GetZDO().m_uid);
        }
    }
}
