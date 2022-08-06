using System.Collections.Generic;
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
    [BepInDependency(MultiUserChest.Plugin.ModGuid, "0.2.0")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "ItemHopper";
        public const string ModGuid = "com.maxsch.valheim.ItemHopper";
        public const string ModVersion = "0.1.1";

        public static ConfigEntry<bool> addSmelterSnappoints;
        public static ConfigEntry<KeyboardShortcut> hopperEditKey;

        private static readonly HashSet<string> HopperPrefabNames = new HashSet<string>();

        public static Plugin Instance { get; private set; }
        public static AssetBundle AssetBundle { get; private set; }

        private Harmony harmony;

        private void Awake() {
            Instance = this;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            addSmelterSnappoints = Config.Bind("General", "Add Smelter Snappoints", true, "Adds snappoints to inputs/outputs of the smelter, charcoal kiln and blastfurnace. Requires a restart to take effect.");
            hopperEditKey = Config.Bind("General", "Hopper Edit Key", new KeyboardShortcut(KeyCode.E, KeyCode.LeftShift), "Key to edit hoppers while hovering over a placed one.");

            CustomLocalization localization = LocalizationManager.Instance.GetLocalization();
            localization.AddJsonFile("English", AssetUtils.LoadTextFromResources("Localization.English.json", Assembly.GetExecutingAssembly()));

            AssetBundle = AssetUtils.LoadAssetBundleFromResources("ValheimHopper_AssetBundle");

            CustomPiece hopperBronzeDown = new CustomPiece(AssetBundle, "HopperBronzeDown", true, BronzeConfig("Wood_V", false));
            CustomPiece hopperBronzeDownFilter = new CustomPiece(AssetBundle, "HopperBronzeDownFilter", true, BronzeConfig("Wood_V", true));
            CustomPiece hopperBronzeSide = new CustomPiece(AssetBundle, "HopperBronzeSide", true, BronzeConfig("Wood_H", false));
            CustomPiece hopperBronzeSideFilter = new CustomPiece(AssetBundle, "HopperBronzeSideFilter", true, BronzeConfig("Wood_H", true));

            CustomPiece hopperIronDown = new CustomPiece(AssetBundle, "HopperIronDown", true, IronConfig("Iron_V", false));
            CustomPiece hopperIronDownFilter = new CustomPiece(AssetBundle, "HopperIronDownFilter", true, IronConfig("Iron_V", true));
            CustomPiece hopperIronSide = new CustomPiece(AssetBundle, "HopperIronSide", true, IronConfig("Iron_H", false));
            CustomPiece hopperIronSideIron = new CustomPiece(AssetBundle, "HopperIronSideFilter", true, IronConfig("Iron_H", true));

            PieceManager.Instance.AddPiece(hopperBronzeDown);
            PieceManager.Instance.AddPiece(hopperBronzeDownFilter);
            PieceManager.Instance.AddPiece(hopperBronzeSide);
            PieceManager.Instance.AddPiece(hopperBronzeSideFilter);

            PieceManager.Instance.AddPiece(hopperIronDown);
            PieceManager.Instance.AddPiece(hopperIronDownFilter);
            PieceManager.Instance.AddPiece(hopperIronSide);
            PieceManager.Instance.AddPiece(hopperIronSideIron);

            HopperPrefabNames.Add(hopperBronzeDown.Piece.name);
            HopperPrefabNames.Add(hopperBronzeDownFilter.Piece.name);
            HopperPrefabNames.Add(hopperBronzeSide.Piece.name);
            HopperPrefabNames.Add(hopperBronzeSideFilter.Piece.name);

            HopperPrefabNames.Add(hopperIronDown.Piece.name);
            HopperPrefabNames.Add(hopperIronDownFilter.Piece.name);
            HopperPrefabNames.Add(hopperIronSide.Piece.name);
            HopperPrefabNames.Add(hopperIronSideIron.Piece.name);

            PrefabManager.OnVanillaPrefabsAvailable += AddSnappoints;
            GUIManager.OnCustomGUIAvailable += HopperUI.Init;
        }

        private static void AddSnappoints() {
            if (addSmelterSnappoints.Value) {
                SnappointHelper.AddSnappoints("smelter", new[] {
                    new Vector3(0f, 1.8f, -1.2f),
                    new Vector3(0f, 1.8f, 1.2f),
                });

                SnappointHelper.AddSnappoints("charcoal_kiln", new[] {
                    new Vector3(0f, 1f, 2.25f),
                });

                SnappointHelper.AddSnappoints("blastfurnace", new[] {
                    new Vector3(-0.5f, 1.72001f, 1.7f),
                    new Vector3(0.57f, 1.72f, 1.70001f),
                });
            }

            PrefabManager.OnVanillaPrefabsAvailable -= AddSnappoints;
        }

        public static bool IsHopperPrefab(GameObject prefab) {
            string name = Utils.GetPrefabName(prefab);
            return HopperPrefabNames.Contains(name);
        }

        private static PieceConfig BronzeConfig(string spriteName, bool filterHopper) {
            return new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>(spriteName),
                Requirements = new[] {
                    new RequirementConfig("Wood", 3, 0, true),
                    new RequirementConfig("BronzeNails", 1, 0, true)
                },
                PieceTable = "Hammer",
                Category = "Crafting",
                Description = filterHopper ? "$hopper_filter_description" : "",
            };
        }

        private static PieceConfig IronConfig(string spriteName, bool filterHopper) {
            return new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>(spriteName),
                Requirements = new[] {
                    new RequirementConfig("Wood", 3, 0, true),
                    new RequirementConfig("IronNails", 1, 0, true)
                },
                PieceTable = "Hammer",
                Category = "Crafting",
                Description = filterHopper ? "$hopper_filter_description" : "",
            };
        }
    }
}
