using AmorLib.Utils;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using GameData;
using GTFO.API;
using GTFO.API.Assets.Profiles;
using GTFO.API.Attributes.ConfigModel;
using GTFO.API.EventArg;
using GTFO.API.Extensions;
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
            [ConfigEntry("Enable verbose logging", false)]
            [Description("Toggles additional debug logging of information to the console")]
            public static ConfigEntry<bool> VerboseLogging { get; private set; }

            [ConfigEntry("Toggle legacy geo fixes", true)]
            [Description("If enabled, apply geomorph fixes (see README for more info). Force disabled if BorkenCellGeoFix is loaded")]
            public static ConfigEntry<bool> LegacyGeoFixesEnabled { get; private set; }

            [ConfigEntry("Precache level assets", true)]
            [Description("Map level assets used for each level in rundown")]
            public static ConfigEntry<bool> PrecacheLevelAssets { get; private set; }

            [ConfigEntry("AssetBundles to load from", BundleLookup)]
            [Description("Comma-separated list of custom AssetBundles to use for asyncronous load during level generation. This should help alleviate the game hanging when loading custom assets")]
            public static ConfigEntry<string> LevelAssetsToLoad { get; private set; }

            [ConfigEntry("Version", ConfigurationVersion)]
            public static ConfigEntry<int> Version { get; private set; }

            public const int ConfigurationVersion = 1;
            public const string BundleLookup = "cavetile-compressed,rlc_legacy-compressed,rlc_mining-compressed,rlc_tech-compressed,customgeo_mining_prelude-compressed,customgeo_service_prelude-compressed,customgeo_tech_prelude-compressed,doggygeos-compressed,dogstilepack-compressed,ds_geo_pack_2-compressed,flowgeo-mining-vol2-compressed,flowgeo-service-vol2-compressed,flowgeo-tech-vol2-compressed,geo_64x64_service_floodways_boss_hub_ds_01-compressed,geo_64x64_service_floodways_hub_ds_01-compressed,geo_64x64_tech_data_center_hub_ds_01-compressed,geo_64x64_tech_data_center_i_tile_ds_1-compressed,geo_mining_flow-compressed,geo_service_flow-compressed,geo_tech_flow-compressed,madgeos-gardens-compressed,qt-geos-mining-compressed,qt-geos-service-compressed,qt-geos-tech-compressed,samgeos-compressed,samgeosv2-compressed,dak_geos-compressed,eternal_geos-compressed,collisiongeo-compressed,customgeo-compressed,mccadcustomgeopack-compressed,yoshi moment gaming-compressed,bro pack 69-compressed,wowoowowowowowoowowrru921r-compressed,dv1_dogcustomgeos-compressed,dogcustomplugs-compressed,dogcustomgeos1-compressed,xiaoyao2-compressed,xiaoyao3-compressed,xiaoyao4-compressed,xiaoyao5-compressed,xiaoyao6-compressed";

        }
#pragma warning restore CS8618
        #endregion

        public const string BCGF_GUID = "com.hirnukuono.BorkenCellGeoFix";
        public static bool BCGF_IsLoaded { get; private set; } = false;
        public static event Action? OnReloaded;
        private static string[] s_levelAssetBundles = Array.Empty<string>();        
        private static readonly HashSet<string> s_levelAssetsToLoad = new(StringComparer.OrdinalIgnoreCase);

        internal static void Prepare(ConfigFile cfg)
        {
            if (IL2CPPChainloader.Instance.Plugins.ContainsKey(BCGF_GUID))
            {
                Logger.Message("BCGF is loaded, legacy geo fixes will be disabled");
                BCGF_IsLoaded = true;
            }

            cfg.ConfigReloaded += (o, args) =>
            {
                s_levelAssetBundles = General.LevelAssetsToLoad.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                OnReloaded?.Invoke();
            };
            ConfigBinder.BindToType(cfg, typeof(ConfigManager));
            SafeFileSystemWatcher.Create(cfg);
            s_levelAssetBundles = General.LevelAssetsToLoad.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            LevelAPI.OnLevelDataUpdated += OnLevelDataUpdated;
            AssetAPI.OnLoadLevelAssets += args =>
            {
                foreach (string asset in s_levelAssetsToLoad)
                {
                    args.EnqueueAssetToLoad(asset);
                }
            };
            AssetAPI.OnAssetBundleLoaded += OnAssetBundleLoaded;
        }

        private static void OnLevelDataUpdated(ActiveExpedition activeExp, ExpeditionInTierData expData)
        {
            s_levelAssetsToLoad.Clear();
            if (expData == null || !General.PrecacheLevelAssets.Value)
                return;

            /* --- read marker datablocks --- */
            var customAssets = new HashSet<string>();
            var markerDBs = new List<IMarkerDataBlock>();
            markerDBs.AddRange(GetAllMarkerBlocks<MiningMarkerDataBlock>());
            markerDBs.AddRange(GetAllMarkerBlocks<TechMarkerDataBlock>());
            markerDBs.AddRange(GetAllMarkerBlocks<ServiceMarkerDataBlock>());
            customAssets.UnionWith(markerDBs.SelectMany(db => db.GetCompositions().ToManaged()).Where(c => c != null).Select(c => c.prefab));

            /* --- check dimension datablocks --- */
            var crsIDs = new HashSet<uint> { expData.Expedition.ComplexResourceData };
            var layoutIDs = new HashSet<uint> { expData.LevelLayoutData, expData.SecondaryLayout, expData.ThirdLayout };
            foreach (var dimExpData in expData.DimensionDatas)
            {
                if (dimExpData.Enabled && DataBlockUtil.TryGetBlock<DimensionDataBlock>(dimExpData.DimensionData, out var dimDB))
                {
                    crsIDs.Add(dimDB.DimensionData.DimensionResourceSetID);
                    layoutIDs.Add(dimDB.DimensionData.LevelLayoutData);
                }
            }

            /* --- read CRS datablocks --- */
            foreach (var id in crsIDs)
            {
                if (!DataBlockUtil.TryGetBlock<ComplexResourceSetDataBlock>(id, out var block))
                    continue;

                var resources = new[]
                {                    
                    block.GeomorphTiles_1x1,
                    block.GeomorphTiles_2x1,
                    block.GeomorphTiles_2x2,
                    block.StraightPlugsNoGates,
                    block.StraightPlugsWithGates,
                    block.SingleDropPlugsNoGates,
                    block.SingleDropPlugsWithGates,             
                    block.DoubleDropPlugsNoGates,
                    block.DoubleDropPlugsWithGates,                   
                    block.PlugCaps,
                    block.ElevatorShafts_1x1,
                    block.CustomGeomorphs_Exit_1x1,
                    block.CustomGeomorphs_Objective_1x1,
                    block.CustomGeomorphs_Challenge_1x1
                };
                customAssets.UnionWith(resources.SelectMany(list => list.ToManaged()).Select(r => r.Prefab));
            }

            /* --- read current LGTuner configs --- */
            foreach (var id in layoutIDs)
            {
                if (!TryGetConfig(id, out var config))
                    continue;

                customAssets.UnionWith(config.ZoneOverrides.SelectMany(z => z.Geomorphs).Select(g => g.Geomorph));
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
            foreach (var prefab in customAssets)
            {
                if (!prefab.IsNullOrWhiteSpace() && s_levelAssetBundles.Any(f => prefab.Contains(f, StringComparison.OrdinalIgnoreCase)))
                    s_levelAssetsToLoad.Add(prefab);                
            }
        }

        private static List<IMarkerDataBlock> GetAllMarkerBlocks<T>() where T : GameDataBlockBase<T>
        {
            var list = new List<IMarkerDataBlock>();
            foreach (var block in GameDataBlockBase<T>.GetAllBlocks())
            {
                if (block == null || block.internalEnabled) 
                    continue;

                var markerBlock = block.TryCast<IMarkerDataBlock>();
                if (markerBlock != null) 
                    list.Add(markerBlock);
            }
            return list;
        }

        private static void OnAssetBundleLoaded(OnAssetBundleLoadedEventArgs args)
        {
            if (!s_levelAssetBundles.Any(f => args.AssetBundleName.Contains(f, StringComparison.OrdinalIgnoreCase)))
                return;

            Logger.Info($"LevelAssetBundle '{args.AssetBundleName}' Loaded!");
            if (args.LoadingOptions.Profile?.ProfileName != "LevelAsset")
                args.LoadingOptions.Profile = new AssetBundleProfile.LevelAsset();
        }
    }
}
