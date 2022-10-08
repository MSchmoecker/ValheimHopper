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
        public const string ModVersion = "0.3.2";

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
            localization.AddJsonFile("German", AssetUtils.LoadTextFromResources("Localization.German.json", Assembly.GetExecutingAssembly()));

            AssetBundle = AssetUtils.LoadAssetBundleFromResources("ValheimHopper_AssetBundle");

            CustomPiece hopperBronzeDown = new CustomPiece(AssetBundle, "HopperBronzeDown", true, BronzeConfig("Wood_V", false));
            CustomPiece hopperBronzeDownFilter = new CustomPiece(AssetBundle, "HopperBronzeDownFilter", true, BronzeConfig("Wood_V", true));
            CustomPiece hopperBronzeSide = new CustomPiece(AssetBundle, "HopperBronzeSide", true, BronzeConfig("Wood_H", false));
            CustomPiece hopperBronzeSideFilter = new CustomPiece(AssetBundle, "HopperBronzeSideFilter", true, BronzeConfig("Wood_H", true));

            CustomPiece hopperIronDown = new CustomPiece(AssetBundle, "HopperIronDown", true, IronConfig("Iron_V", false));
            CustomPiece hopperIronDownFilter = new CustomPiece(AssetBundle, "HopperIronDownFilter", true, IronConfig("Iron_V", true));
            CustomPiece hopperIronSide = new CustomPiece(AssetBundle, "HopperIronSide", true, IronConfig("Iron_H", false));
            CustomPiece hopperIronSideFilter = new CustomPiece(AssetBundle, "HopperIronSideFilter", true, IronConfig("Iron_H", true));

            PieceManager.Instance.AddPiece(hopperBronzeDown);
            PieceManager.Instance.AddPiece(hopperBronzeDownFilter);
            PieceManager.Instance.AddPiece(hopperBronzeSide);
            PieceManager.Instance.AddPiece(hopperBronzeSideFilter);

            PieceManager.Instance.AddPiece(hopperIronDown);
            PieceManager.Instance.AddPiece(hopperIronDownFilter);
            PieceManager.Instance.AddPiece(hopperIronSide);
            PieceManager.Instance.AddPiece(hopperIronSideFilter);

            hopperBronzeDownFilter.Piece.m_enabled = false;
            hopperBronzeSideFilter.Piece.m_enabled = false;
            hopperIronDownFilter.Piece.m_enabled = false;
            hopperIronSideFilter.Piece.m_enabled = false;

            PrefabManager.OnVanillaPrefabsAvailable += AddSnappoints;
            GUIManager.OnCustomGUIAvailable += HopperUI.Init;
        }

        private static void AddSnappoints() {
            if (addSmelterSnappoints.Value) {
                SnappointHelper.AddSnappoints("smelter", new[] {
                    new Vector3(0f, 1.6f, -1.2f),
                    new Vector3(0f, 1.6f, 1.2f),
                });

                SnappointHelper.AddSnappoints("charcoal_kiln", new[] {
                    new Vector3(0f, 1f, 2.15f),
                });

                SnappointHelper.AddSnappoints("blastfurnace", new[] {
                    new Vector3(-0.5f, 1.72001f, 1.55f),
                    new Vector3(-0.6f, 1.72001f, 1.55f),
                    new Vector3(0.57f, 1.72f, 1.55001f),
                    new Vector3(0.73f, 1.72f, 1.55001f),
                });
            }

            PrefabManager.OnVanillaPrefabsAvailable -= AddSnappoints;
        }

        private static PieceConfig BronzeConfig(string spriteName, bool filterHopper) {
            return new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>(spriteName),
                Requirements = new[] {
                    new RequirementConfig("Wood", 3, 0, true),
                    new RequirementConfig("BronzeNails", 1, 0, true)
                },
                PieceTable = "Hammer",
                CraftingStation = "piece_workbench",
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
                CraftingStation = "piece_workbench",
                Category = "Crafting",
                Description = filterHopper ? "$hopper_filter_description" : "",
            };
        }
    }
}
