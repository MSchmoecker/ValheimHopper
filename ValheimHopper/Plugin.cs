using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
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
        [PublicAPI] public const string ModName = "ItemHopper";
        [PublicAPI] public const string ModGuid = "com.maxsch.valheim.ItemHopper";
        [PublicAPI] public const string ModVersion = "1.4.1";

        private static ConfigEntry<bool> addSmelterSnappoints;

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
            localization.AddJsonFile("Russian", AssetUtils.LoadTextFromResources("Localization.Russian.json", Assembly.GetExecutingAssembly()));

            AssetBundle = AssetUtils.LoadAssetBundleFromResources("ValheimHopper_AssetBundle");

            AddBronzePiece("HopperBronzeDown", 6, 4);
            AddBronzePiece("HopperBronzeSide", 6, 4);
            AddBronzePiece("MS_PipeBronzeSide", 4, 2);
            AddBronzePiece("MS_PipeBronzeSide_2m", 2, 1);
            AddBronzePiece("MS_PipeBronze_Vertical_Up_4m", 4, 2);
            AddBronzePiece("MS_PipeBronze_Vertical_Down_4m", 4, 2);
            AddBronzePiece("MS_PipeBronze_Vertical_Up_2m", 2, 1);
            AddBronzePiece("MS_PipeBronze_Vertical_Down_2m", 2, 1);
            AddBronzePiece("MS_PipeBronze_Diagonal_45_Up_4m", 4, 2);
            AddBronzePiece("MS_PipeBronze_Diagonal_45_Down_4m", 4, 2);
            AddBronzePiece("MS_PipeBronze_Diagonal_26_Up_4m", 4, 2);
            AddBronzePiece("MS_PipeBronze_Diagonal_26_Down_4m", 4, 2);
            AddIronPiece("HopperIronDown", 6, 2);
            AddIronPiece("HopperIronSide", 6, 2);

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
                    new Vector3(0f, 1.1f, 2f),
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

        private static void AddBronzePiece(string assetName, int wood, int nails) {
            PieceConfig config = new PieceConfig {
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

        private static void AddIronPiece(string assetName, int wood, int nails) {
            PieceConfig config = new PieceConfig {
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
