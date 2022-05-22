using System.Reflection;
using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;
using Jotunn.Utils;
using Jotunn.Managers;

namespace ValheimHopper {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency(MultiUserChest.Plugin.ModGuid)]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "ValheimHopper";
        public const string ModGuid = "com.maxsch.valheim.ValheimHopper";
        public const string ModVersion = "0.0.0";

        public static Plugin Instance { get; private set; }
        public static AssetBundle AssetBundle { get; private set; }

        private Harmony harmony;

        private void Awake() {
            Instance = this;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            // load embedded asset bundle
            AssetBundle = AssetUtils.LoadAssetBundleFromResources("ValheimHopper_AssetBundle", Assembly.GetExecutingAssembly());

            // load embedded localisation
            CustomLocalization localization = new CustomLocalization();
            string englishJson = AssetUtils.LoadTextFromResources("Localization.English.json", Assembly.GetExecutingAssembly());
            localization.AddJsonFile("English", englishJson);
            LocalizationManager.Instance.AddLocalization(localization);

            GameObject hopperDown = AssetBundle.LoadAsset<GameObject>("HopperDown");
            GameObject hopperSide = AssetBundle.LoadAsset<GameObject>("HopperSide");
            GameObject hopperDownMetal = AssetBundle.LoadAsset<GameObject>("HopperDownMetal");
            GameObject hopperSideMetal = AssetBundle.LoadAsset<GameObject>("HopperSideMetal");

            CustomPiece hopperPiece = new CustomPiece(hopperDown, true, new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>("Wood_V"),
                Requirements = new[] {
                    new RequirementConfig { Item = "Wood", Amount = 2, Recover = true }
                },
                PieceTable = "Hammer"
            });

            CustomPiece hopperSidePiece = new CustomPiece(hopperSide, true, new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>("Wood_H"),
                Requirements = new[] {
                    new RequirementConfig { Item = "Wood", Amount = 3, Recover = true }
                },
                PieceTable = "Hammer"
            });

            CustomPiece hopperPieceMetal = new CustomPiece(hopperDownMetal, true, new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>("Iron_V"),
                Requirements = new[] {
                    new RequirementConfig { Item = "Wood", Amount = 3, Recover = true }
                },
                PieceTable = "Hammer"
            });

            CustomPiece hopperSidePieceMetal = new CustomPiece(hopperSideMetal, true, new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>("Iron_H"),
                Requirements = new[] {
                    new RequirementConfig { Item = "Wood", Amount = 3, Recover = true }
                },
                PieceTable = "Hammer"
            });

            PieceManager.Instance.AddPiece(hopperPiece);
            PieceManager.Instance.AddPiece(hopperSidePiece);
            PieceManager.Instance.AddPiece(hopperPieceMetal);
            PieceManager.Instance.AddPiece(hopperSidePieceMetal);
        }

        public static bool IsHopperPrefab(GameObject prefab) {
            string name = Utils.GetPrefabName(prefab);
            return name == "HopperDown" || name == "HopperSide" || name == "HopperDownMetal" || name == "HopperSideMetal";
        }
    }
}
