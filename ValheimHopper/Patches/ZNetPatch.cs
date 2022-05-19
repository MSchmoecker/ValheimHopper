using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace ValheimHopper.Patches {
    [HarmonyPatch]
    public class ZNetPatch {
        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake)), HarmonyPostfix]
        public static void AfterZNetSceneAwake() {
            SnappointHelper.AddSnappoints("smelter", new[] {
                new Vector3(0f, 1.9f, -1.4f),
                new Vector3(0f, 1.9f, 1.4f),
                new Vector3(0f, 1.9f, 1.2f),
                new Vector3(0f, 1.9f, 1.6f),
            });

            SnappointHelper.AddSnappoints("piece_chest_wood", new[] {
                new Vector3(0f, 0, 0f),
                new Vector3(0f, 0.85f, 0f),
            });

            PrefabManager.Cache.GetPrefab<WearNTear>("piece_chest_wood").m_supports = true;
        }
    }
}
