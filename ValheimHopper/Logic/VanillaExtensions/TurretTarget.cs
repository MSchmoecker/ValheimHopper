using System;
using UnityEngine;
using ValheimHopper.Logic.Helper;

namespace ValheimHopper.Logic {
    public class TurretTarget : NetworkPiece, IPushTarget {
        public HopperPriority PushPriority { get; } = HopperPriority.TurretPush;

        private Turret turret;

        protected override void Awake() {
            base.Awake();
            turret = GetComponent<Turret>();
        }

        public void AddItem(ItemDrop.ItemData item, Inventory source, ZDOID sender) {
            bool removed = source.RemoveItem(item, 1);

            if (!removed) {
                return;
            }

            turret.m_nview.InvokeRPC("RPC_AddAmmo", item.m_dropPrefab.name);
        }

        public bool CanAddItem(ItemDrop.ItemData item) {
            if (!turret.IsItemAllowed(item.m_dropPrefab.name)) {
                return false;
            }

            if (turret.GetAmmo() > 0 && turret.GetAmmoType() != item.m_dropPrefab.name) {
                return false;
            }

            if (turret.GetAmmo() >= turret.m_maxAmmo) {
                return false;
            }

            return true;
        }

        public bool InRange(Vector3 position) {
            return true;
        }
    }
}
