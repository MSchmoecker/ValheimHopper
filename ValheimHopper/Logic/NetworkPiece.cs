using System;
using UnityEngine;
using ValheimHopper.Logic.Helper;

namespace ValheimHopper.Logic {
    public class NetworkPiece : MonoBehaviour {
        protected ZNetView zNetView;

        protected virtual void Awake() {
            zNetView = GetComponentInParent<ZNetView>();
        }

        public bool IsValid() {
            return this && zNetView && HopperHelper.IsValidNetView(zNetView) && zNetView.HasOwner();
        }

        public int NetworkHashCode() {
            return HopperHelper.GetNetworkHashCode(zNetView);
        }

        public bool Equals(ITarget x, ITarget y) {
            return x == y || x?.NetworkHashCode() == y?.NetworkHashCode();
        }

        public int GetHashCode(ITarget obj) {
            return obj.NetworkHashCode();
        }
    }
}
