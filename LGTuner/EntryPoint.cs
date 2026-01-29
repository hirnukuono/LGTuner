using AssetShards;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using LevelGeneration;
using LGTuner.Manager;
using LGTuner.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LGTuner
{
    [BepInPlugin("LGTuner", "LGTuner", "1.2.0")]
    [BepInProcess("GTFO.exe")]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFOUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(MTFOPartialDataUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class EntryPoint : BasePlugin
    {
        public static Dictionary<string, AssetBundle> BundleLookup = new();
        public static string CONFIG_PATH = Path.Combine(Paths.ConfigPath, "LGTuner.cfg");
        public static ConfigFile config = new(CONFIG_PATH, true);
        public static ConfigEntry<string> _bundlesToRename;
        public static ConfigEntry<string> _bundlesToLoadAllFrom;
        public static List<AssetBundle> BundleLoadAllLookup = new();
        public static bool StairsFixed = false;
        public static bool TechFixed = false;
        public static bool ServiceFixed = false;
        public static List<string> CustomMarkerPrefabs = new();

        public override void Load()
        {
            _bundlesToRename = config.Bind(new ConfigDefinition("General","Bundles_To_Rename"), "cavetile-compressed,rlc_legacy-compressed,rlc_mining-compressed,rlc_tech-compressed,customgeo_mining_prelude-compressed,customgeo_service_prelude-compressed,customgeo_tech_prelude-compressed,doggygeos-compressed,dogstilepack-compressed,ds_geo_pack_2-compressed,flowgeo-mining-vol2-compressed,flowgeo-service-vol2-compressed,flowgeo-tech-vol2-compressed,geo_64x64_service_floodways_boss_hub_ds_01-compressed,geo_64x64_service_floodways_hub_ds_01-compressed,geo_64x64_tech_data_center_hub_ds_01-compressed,geo_64x64_tech_data_center_i_tile_ds_1-compressed,geo_mining_flow-compressed,geo_service_flow-compressed,geo_tech_flow-compressed,madgeos-gardens-compressed,qt-geos-mining-compressed,qt-geos-service-compressed,qt-geos-tech-compressed,samgeos-compressed,samgeosv2-compressed,dak_geos-compressed,eternal_geos-compressed,collisiongeo-compressed,customgeo-compressed,mccadcustomgeopack-compressed,yoshi moment gaming-compressed,bro pack 69-compressed,wowoowowowowowoowowrru921r-compressed,dv1_dogcustomgeos-compressed,dogcustomplugs-compressed,dogcustomgeos1-compressed", new ConfigDescription("comma separated list of asset bundle files for lgtuner processing (will be renamed file.fdfd.manifest)"));
            _bundlesToLoadAllFrom = config.Bind(new ConfigDefinition("General", "Bundles_To_Load_All_From"), "somefile1-compressed,somefile2-compressed,somefile3-compressed", new ConfigDescription("comma separated list of asset bundle files that will be fully loaded upon use, if problems with single prefab loading arise"));
            HarmonyInstance = new Harmony("LGTuner");
            HarmonyInstance.PatchAll();
            ConfigManager.RenameFiles();
            ConfigManager.Init();
            AssetShardManager.add_OnSharedAsssetLoaded((Action)ConfigManager.LoadCustomBundles);
            LG_Factory.add_OnFactoryBuildStart((Action)ConfigManager.LoadData);
            LG_Factory.add_OnFactoryBuildDone((Action)ConfigManager.UnloadData);
        }

        public Harmony HarmonyInstance { get; private set; }
    }
}