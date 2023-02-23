using UnityEngine;

namespace ValheimHopper.Logic {
    public interface ITarget {
        bool IsValid();
        bool InRange(Vector3 position);
    }
}
