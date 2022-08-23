using System;
using UnityEngine;

namespace ValheimHopper {
    [Serializable]
    public class TargetIO {
        public Piece piece;
        public Container container;
        public Smelter smelter;
        public Hopper hopper;

        public TargetIO(Piece piece) {
            this.piece = piece;
            container = piece.GetComponent<Container>();
            smelter = piece.GetComponent<Smelter>();
            hopper = piece.GetComponent<Hopper>();
        }
    }
}
