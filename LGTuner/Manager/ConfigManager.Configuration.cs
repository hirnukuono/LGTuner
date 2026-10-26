using AmorLib.Utils;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using GameData;
using GTFO.API;
using GTFO.API.Attributes.ConfigModel;
using GTFO.API.Utilities;
using System.ComponentModel;

namespace LGTuner.Manager
{
    public static partial class ConfigManager
    {
        #region CONFIGURATION
        #pragma warning disable CS8618 
        [Section("General")]
        internal static class General
        {
            public const int ConfigurationVersion = 1;
            public const string BundleLookup = "cavetile-compressed,rlc_legacy-compressed,rlc_mining-compressed,rlc_tech-compressed,customgeo_mining_prelude-compressed,customgeo_service_prelude-compressed,customgeo_tech_prelude-compressed,doggygeos-compressed,dogstilepack-compressed,ds_geo_pack_2-compressed,flowgeo-mining-vol2-compressed,flowgeo-service-vol2-compressed,flowgeo-tech-vol2-compressed,geo_64x64_service_floodways_boss_hub_ds_01-compressed,geo_64x64_service_floodways_hub_ds_01-compressed,geo_64x64_tech_data_center_hub_ds_01-compressed,geo_64x64_tech_data_center_i_tile_ds_1-compressed,geo_mining_flow-compressed,geo_service_flow-compressed,geo_tech_flow-compressed,madgeos-gardens-compressed,qt-geos-mining-compressed,qt-geos-service-compressed,qt-geos-tech-compressed,samgeos-compressed,samgeosv2-compressed,dak_geos-compressed,eternal_geos-compressed,collisiongeo-compressed,customgeo-compressed,mccadcustomgeopack-compressed,yoshi moment gaming-compressed,bro pack 69-compressed,wowoowowowowowoowowrru921r-compressed,dv1_dogcustomgeos-compressed,dogcustomplugs-compressed,dogcustomgeos1-compressed,xiaoyao2-compressed,xiaoyao3-compressed,xiaoyao4-compressed,xiaoyao5-compressed,xiaoyao6-compressed";

            [ConfigEntry("Enable verbose logging", false)]
            [Description("Toggles additional debug logging of information to the console")]
            public static ConfigEntry<bool> VerboseEnabled { get; private set; }

            [ConfigEntry("Toggle legacy geo fixes", true)]
            [Description("If enabled, apply geomorph fixes (see README for more info). Force disabled if BorkenCellGeoFix is loaded")]
            public static ConfigEntry<bool> LegacyGeoFixesEnabled { get; private set; }

            [ConfigEntry("Precache custom level assets", true)]
            [Description("Map level assets used for each level in LGTuner overrides. This should help alleviate the game hanging when loading custom assets")]
            public static ConfigEntry<bool> PrecacheLevelAssets { get; private set; }

            [ConfigEntry("AssetBundle lookup", BundleLookup)]
            [Description("Comma-separated list of custom AssetBundles to use for asyncronous load during level generation")]
            public static ConfigEntry<string> LevelAssetsToLoad { get; private set; }

            [ConfigEntry("Version", ConfigurationVersion)]
            public static ConfigEntry<int> Version { get; private set; }
        }
#pragma warning restore CS8618
        #endregion

        public const string BCGF_GUID = "com.hirnukuono.BorkenCellGeoFix";
        public static bool BCGF_IsLoaded { get; private set; } = false;
        private static string[] s_levelAssetBundles = Array.Empty<string>();        
        private static readonly HashSet<string> s_levelAssets = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> s_levelAssetsToLoad = new(StringComparer.OrdinalIgnoreCase);

        internal static void Prepare(ConfigFile cfg)
        {
            if (IL2CPPChainloader.Instance.Plugins.ContainsKey(BCGF_GUID))
            {
                Logger.Message("BCGF is loaded, legacy geo fixes will be force disabled");
                BCGF_IsLoaded = true;
            }

            cfg.ConfigReloaded += (o, args) => OnAssetBundlesLoaded();
            ConfigBinder.BindToType(cfg, typeof(ConfigManager));
            SafeFileSystemWatcher.Create(cfg);

            AssetAPI.OnAssetBundlesLoaded += OnAssetBundlesLoaded;
            LevelAPI.OnLevelDataUpdated += OnLevelDataUpdated;
            AssetAPI.OnLoadLevelAssets += args =>
            {
                foreach (string asset in s_levelAssetsToLoad)
                {
                    args.EnqueueAssetToLoad(asset);
                }
            };
        }

        private static void OnAssetBundlesLoaded()
        {
            s_levelAssetBundles = General.LevelAssetsToLoad.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            s_levelAssets.Clear();
            foreach ((string bundle, var bundleRef) in AssetAPI.AllCustomBundles.KeyValues)
            {
                if (s_levelAssetBundles.Any(f => bundle.Contains(f, StringComparison.OrdinalIgnoreCase)))
                    s_levelAssets.UnionWith(bundleRef.Assets.AssetNames);
            }
        }

        private static void OnLevelDataUpdated(ActiveExpedition activeExp, ExpeditionInTierData expData)
        {
            s_levelAssetsToLoad.Clear();
            if (expData == null || !General.PrecacheLevelAssets.Value)
                return;

            /* --- get current layout datablocks --- */
            var layoutIDs = new HashSet<uint> { expData.LevelLayoutData, expData.SecondaryLayout, expData.ThirdLayout };
            foreach (var dimExpData in expData.DimensionDatas)
            {
                if (dimExpData.Enabled && DataBlockUtil.TryGetBlock<DimensionDataBlock>(dimExpData.DimensionData, out var dimDB))
                    layoutIDs.Add(dimDB.DimensionData.LevelLayoutData);                
            }

            /* --- read current configs --- */
            var customAssets = new HashSet<string>();
            foreach (uint id in layoutIDs)
            {
                if (!TryGetConfig(id, out var config))
                    continue;

                customAssets.UnionWith(config.ZoneOverrides.SelectMany(z => z.Geomorphs).Select(g => g.Geomorph));
                customAssets.UnionWith(config.ZoneOverrides.SelectMany(z => z.Plugs));
                foreach (var t in config.TileOverrides)
                {
                    customAssets.Add(t.Geomorph);
                    customAssets.Add(t.ForwardPlug);
                    customAssets.Add(t.BackwardPlug);
                    customAssets.Add(t.LeftPlug);
                    customAssets.Add(t.RightPlug);
                }
            }

            /* --- add to assets to load --- */
            foreach (string prefab in customAssets)
            {
                if (!prefab.IsNullOrWhiteSpace() && s_levelAssets.Any(f => prefab.Equals(f, StringComparison.OrdinalIgnoreCase)))
                    s_levelAssetsToLoad.Add(prefab);                
            }
        }
    }
}
