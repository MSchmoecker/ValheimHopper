using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace ValheimHopper.Patches {
    [HarmonyPatch]
    public class ZNetPatch {
        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake)), HarmonyPostfix]
        public static void AfterZNetSceneAwake() {
            SnappointHelper.AddSnappoints("smelter", new[] {
                new Vector3(0f, 1.8f, -1.2f),
                new Vector3(0f, 1.8f, 1.2f),
            });
        }
    }
}
