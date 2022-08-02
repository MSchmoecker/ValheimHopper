using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
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

        public static ConfigEntry<bool> addSmelterSnappoints;

        public static Plugin Instance { get; private set; }
        public static AssetBundle AssetBundle { get; private set; }

        private Harmony harmony;

        private void Awake() {
            Instance = this;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            addSmelterSnappoints = Config.Bind("General", "Add Smelter Snappoints", true, "Adds snappoints to inputs/outputs of the smelter, charcoal kiln and blastfurnace. Requires a restart to take effect.");

            CustomLocalization localization = LocalizationManager.Instance.GetLocalization();
            localization.AddJsonFile("English", AssetUtils.LoadTextFromResources("Localization.English.json", Assembly.GetExecutingAssembly()));

            AssetBundle = AssetUtils.LoadAssetBundleFromResources("ValheimHopper_AssetBundle");

            CustomPiece hopperDownBronze = new CustomPiece(AssetBundle, "HopperDown", true, BronzeConfig("Wood_V"));
            CustomPiece hopperSideBronze = new CustomPiece(AssetBundle, "HopperSide", true, BronzeConfig("Wood_H"));
            CustomPiece hopperDownIron = new CustomPiece(AssetBundle, "HopperDownMetal", true, IronConfig("Iron_V"));
            CustomPiece hopperSideIron = new CustomPiece(AssetBundle, "HopperSideMetal", true, IronConfig("Iron_H"));

            PieceManager.Instance.AddPiece(hopperDownBronze);
            PieceManager.Instance.AddPiece(hopperSideBronze);
            PieceManager.Instance.AddPiece(hopperDownIron);
            PieceManager.Instance.AddPiece(hopperSideIron);

            PrefabManager.OnVanillaPrefabsAvailable += AddSnappoints;
        }

        private void AddSnappoints() {
            if (addSmelterSnappoints.Value) {
                SnappointHelper.AddSnappoints("smelter", new[] {
                    new Vector3(0f, 1.8f, -1.2f),
                    new Vector3(0f, 1.8f, 1.2f),
                });

                SnappointHelper.AddSnappoints("charcoal_kiln", new[] {
                    new Vector3(0f, 1f, 2.25f),
                });

                SnappointHelper.AddSnappoints("blastfurnace", new[] {
                    new Vector3(-0.5f, 1.72f, 1.7f),
                    new Vector3(0.55f, 1.72f, 1.7f),
                });
            }

            PrefabManager.OnVanillaPrefabsAvailable -= AddSnappoints;
        }

        public static bool IsHopperPrefab(GameObject prefab) {
            string name = Utils.GetPrefabName(prefab);
            return name == "HopperDown" || name == "HopperSide" || name == "HopperDownMetal" || name == "HopperSideMetal";
        }

        private static PieceConfig BronzeConfig(string spriteName) {
            return new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>(spriteName),
                Requirements = new[] {
                    new RequirementConfig("Wood", 3, 0, true),
                    new RequirementConfig("BronzeNails", 1, 0, true)
                },
                PieceTable = "Hammer",
                Category = "Crafting",
            };
        }

        private static PieceConfig IronConfig(string spriteName) {
            return new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>(spriteName),
                Requirements = new[] {
                    new RequirementConfig("Wood", 3, 0, true),
                    new RequirementConfig("IronNails", 1, 0, true)
                },
                PieceTable = "Hammer",
                Category = "Crafting",
            };
        }
    }
}
