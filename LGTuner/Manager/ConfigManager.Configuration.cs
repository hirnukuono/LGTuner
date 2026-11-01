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
        #pragma warning disable CS8618 
        [Section("General")]
        internal static class General
        {
            [ConfigEntry("Enable verbose logging", false)]
            [Description("Toggles additional debug logging of information to the console")]
            public static ConfigEntry<bool> VerboseEnabled { get; private set; }

            [ConfigEntry("Toggle legacy geo fixes", true)]
            [Description("If enabled, apply geomorph fixes (see README for more info). Is internally disabled if BorkenCellGeoFix is also loaded")]
            public static ConfigEntry<bool> LegacyGeoFixesEnabled { get; private set; }

            [ConfigEntry("Precache LGTuner level assets", true)]
            [Description("Map level assets used in LGTuner overrides. This should help alleviate the game hanging when loading assets")]
            public static ConfigEntry<bool> PrecacheLevelAssets { get; private set; }

            [ConfigEntry("Version", 2)]
            public static ConfigEntry<int> Version { get; private set; }
        }
        #pragma warning restore CS8618

        public const string BCGF_GUID = "com.hirnukuono.BorkenCellGeoFix";
        public static bool BCGF_IsLoaded { get; private set; } = false;
        private static readonly HashSet<string> s_levelAssetsToLoad = new(StringComparer.OrdinalIgnoreCase);

        internal static void Prepare(ConfigFile cfg)
        {
            if (IL2CPPChainloader.Instance.Plugins.ContainsKey(BCGF_GUID))
            {
                Logger.Message("BCGF is loaded, legacy geo fixes will be force disabled");
                BCGF_IsLoaded = true;
            }

            cfg.ConfigReloaded += (o, args) => Logger.Warn("Config file changed: " + cfg.ConfigFilePath);
            ConfigBinder.BindToType(cfg, typeof(ConfigManager));
            SafeFileSystemWatcher.Create(cfg);

            LevelAPI.OnLevelDataUpdated += OnLevelDataUpdated;
            AssetAPI.OnLoadLevelAssets += args =>
            {
                foreach (string asset in s_levelAssetsToLoad.Where(f => !f.IsNullOrWhiteSpace())) 
                {
                    args.EnqueueAssetToLoad(asset);
                }
            };
        }

        private static void OnLevelDataUpdated(ActiveExpedition activeExp, ExpeditionInTierData expData)
        {
            s_levelAssetsToLoad.Clear();
            if (expData == null || !General.PrecacheLevelAssets.Value)
                return;

            var layoutIDs = new HashSet<uint> { expData.LevelLayoutData, expData.SecondaryLayout, expData.ThirdLayout };
            foreach (var dimExpData in expData.DimensionDatas)
            {
                if (dimExpData.Enabled && DataBlockUtil.TryGetBlock<DimensionDataBlock>(dimExpData.DimensionData, out var dimDB))
                    layoutIDs.Add(dimDB.DimensionData.LevelLayoutData);                
            }

            foreach (uint id in layoutIDs)
            {
                if (!TryGetConfig(id, out var config))
                    continue;

                s_levelAssetsToLoad.UnionWith(config.ZoneOverrides.SelectMany(z => z.Geomorphs).Select(g => g.Geomorph));
                s_levelAssetsToLoad.UnionWith(config.ZoneOverrides.SelectMany(z => z.Plugs));
                foreach (var t in config.TileOverrides)
                {
                    s_levelAssetsToLoad.Add(t.Geomorph);
                    s_levelAssetsToLoad.Add(t.ForwardPlug);
                    s_levelAssetsToLoad.Add(t.BackwardPlug);
                    s_levelAssetsToLoad.Add(t.LeftPlug);
                    s_levelAssetsToLoad.Add(t.RightPlug);
                }
            }
        }
    }
}
