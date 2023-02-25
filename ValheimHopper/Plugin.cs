﻿using System.Collections.Generic;
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

            AddBronzePiece("HopperBronzeDown", "Wood_V");
            AddBronzePiece("HopperBronzeSide", "Wood_H");
            AddIronPiece("HopperIronDown", "Iron_V");
            AddIronPiece("HopperIronSide", "Iron_H");
            AddBronzePiece("MS_PipeBronzeSide", "Pipe_H");

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

        private static void AddBronzePiece(string assetName, string spriteName) {
            PieceConfig config = new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>(spriteName),
                Requirements = new[] {
                    new RequirementConfig("Wood", 3, 0, true),
                    new RequirementConfig("BronzeNails", 1, 0, true)
                },
                PieceTable = "Hammer",
                CraftingStation = "piece_workbench",
                Category = "Crafting",
            };

            PieceManager.Instance.AddPiece(new CustomPiece(AssetBundle, assetName, true, config));
        }

        private static void AddIronPiece(string assetName, string spriteName) {
            PieceConfig config = new PieceConfig {
                Icon = AssetBundle.LoadAsset<Sprite>(spriteName),
                Requirements = new[] {
                    new RequirementConfig("Wood", 3, 0, true),
                    new RequirementConfig("IronNails", 1, 0, true)
                },
                PieceTable = "Hammer",
                CraftingStation = "piece_workbench",
                Category = "Crafting",
            };

            PieceManager.Instance.AddPiece(new CustomPiece(AssetBundle, assetName, true, config));
        }
    }
}
