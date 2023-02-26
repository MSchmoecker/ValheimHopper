using System.Collections.Generic;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace ValheimHopper.Logic.Helper {
    public static class ItemHelper {
        private static readonly Dictionary<string, Vector3> ItemOffsetCache = new Dictionary<string, Vector3>();

        public static Vector3 GetVisualItemOffset(string name) {
            if (ItemOffsetCache.TryGetValue(name, out Vector3 center)) {
                return center;
            }

            GameObject item = ObjectDB.instance.GetItemPrefab(name.GetStableHashCode());

            if (!item) {
                Logger.LogWarning($"Could not find item {name} for offset calculation");
                ItemOffsetCache[name] = Vector3.zero;
                return ItemOffsetCache[name];
            }

            Vector3 min = new Vector3(1000f, 1000f, 1000f);
            Vector3 max = new Vector3(-1000f, -1000f, -1000f);
            Vector3 parentPos = item.transform.position;

            foreach (Renderer meshRenderer in item.GetComponentsInChildren<Renderer>()) {
                if (meshRenderer is ParticleSystemRenderer) {
                    continue;
                }

                min = Vector3.Min(min, parentPos - meshRenderer.bounds.min);
                max = Vector3.Max(max,  parentPos - meshRenderer.bounds.max);
            }

            center = (min + max) / 2f;
            ItemOffsetCache[name] = center;
            return center;
        }

        public static void CheckDropPrefab(ItemDrop itemDrop) {
            if (!itemDrop.m_itemData.m_dropPrefab) {
                itemDrop.m_itemData.m_dropPrefab = ObjectDB.instance.GetItemPrefab(itemDrop.GetPrefabName(itemDrop.gameObject.name));
            }
        }
    }
}
