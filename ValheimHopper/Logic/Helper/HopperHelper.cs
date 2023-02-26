using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ValheimHopper.Logic.Helper {
    public static class HopperHelper {
        private static readonly Collider[] tempColliders = new Collider[256];
        private static int PieceMask { get; } = LayerMask.GetMask("piece", "piece_nonsolid");
        private static int ItemMask { get; } = LayerMask.GetMask("item");

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

        public static int GetFixedFrameCount() {
            return Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime);
        }

        public static List<T> FindTargets<T>(Vector3 pos, Vector3 size, Quaternion rotation, Func<T, int> orderBy) where T : ITarget {
            List<T> targets = new List<T>();
            int count = Physics.OverlapBoxNonAlloc(pos, size / 2f, tempColliders, rotation, PieceMask | ItemMask);

            for (int i = 0; i < count; i++) {
                targets.AddRange(tempColliders[i].GetComponentsInParent<T>().Where(t => t.InRange(pos)));
            }

            return targets.Distinct().OrderByDescending(orderBy).ToList();
        }
    }
}
