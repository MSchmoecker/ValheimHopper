using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Utils;
using Jotunn.Managers;
using ValheimHopper.Logic.Helper;
using ValheimHopper.UI;

namespace ValheimHopper {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid, "2.7.7")]
    [BepInDependency(MultiUserChest.Plugin.ModGuid, "0.4.0")]
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

            addSmelterSnappoints = Config.Bind("General", "Add Smelter Snappoints", true, "Adds snappoints to inputs/outputs of the smelter, charcoal kiln, blastfurnace, windmill and spinning wheel. Requires a restart to take effect.");

            CustomLocalization localization = LocalizationManager.Instance.GetLocalization();
            localization.AddJsonFile("English", AssetUtils.LoadTextFromResources("Localization.English.json", Assembly.GetExecutingAssembly()));
            localization.AddJsonFile("German", AssetUtils.LoadTextFromResources("Localization.German.json", Assembly.GetExecutingAssembly()));

            AssetBundle = AssetUtils.LoadAssetBundleFromResources("ValheimHopper_AssetBundle");

            AddBronzePiece("HopperBronzeDown", "Wood_V", 6, 4);
            AddBronzePiece("HopperBronzeSide", "Wood_H", 6, 4);
            AddIronPiece("HopperIronDown", "Iron_V", 8, 2);
            AddIronPiece("HopperIronSide", "Iron_H", 8, 2);
            AddBronzePiece("VH_PipeBronzeSide", "Pipe_H", 4, 2);

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

                SnappointHelper.AddSnappoints("windmill", new[] {
                    new Vector3(0f, 1.55f, -1.55f),
                    new Vector3(-0.05f, 0.83f, 2.3f),
                });

                SnappointHelper.AddSnappoints("piece_spinningwheel", new[] {
                    new Vector3(0.72f, 1.8f, 0f),
                    new Vector3(0f, 0.95f, 1.75f),
                });
            }

            PrefabManager.OnVanillaPrefabsAvailable -= AddSnappoints;
        }

        private static void AddBronzePiece(string assetName, string spriteName, int wood, int nails) {
            PieceConfig config = new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>(spriteName),
                Requirements = new[] {
                    new RequirementConfig("Wood", wood, 0, true),
                    new RequirementConfig("BronzeNails", nails, 0, true)
                },
                PieceTable = "Hammer",
                CraftingStation = "piece_workbench",
                Category = "Crafting",
            };

            PieceManager.Instance.AddPiece(new CustomPiece(AssetBundle, assetName, true, config));
        }

        private static void AddIronPiece(string assetName, string spriteName, int wood, int nails) {
            PieceConfig config = new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>(spriteName),
                Requirements = new[] {
                    new RequirementConfig("Wood", wood, 0, true),
                    new RequirementConfig("IronNails", nails, 0, true)
                },
                PieceTable = "Hammer",
                CraftingStation = "piece_workbench",
                Category = "Crafting",
            };

            PieceManager.Instance.AddPiece(new CustomPiece(AssetBundle, assetName, true, config));
        }
    }
}
