using UnityEngine;

namespace ValheimHopper.Logic {
    public static class Helper {
        public static bool IsInRange(Vector3 position, Vector3 target, float range) {
            return Vector3.SqrMagnitude(target - position) < range * range;
        }

        public static bool IsInRange(Vector3 position, Collider collider, float range) {
            Bounds bounds = collider.bounds;
            bounds.Expand(range);
            return bounds.Contains(position);
        }

        public static bool IsValidNetView(ZNetView netView) {
            return netView && netView.IsValid() && netView.m_zdo != null;
        }
    }
}
