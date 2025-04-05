using AssetShards;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using LGTuner.Manager;
using LGTuner.Utils;
using System;

namespace LGTuner
{
    [BepInPlugin("LGTuner", "LGTuner", "1.1.3")]
    [BepInProcess("GTFO.exe")]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFOUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(MTFOPartialDataUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class EntryPoint : BasePlugin
    {
        public static bool AssetsFixed = false; 
        public override void Load()
        {
            HarmonyInstance = new Harmony("LGTuner");
            HarmonyInstance.PatchAll();
            ConfigManager.Init();
            AssetShardManager.add_OnSharedAsssetLoaded((Action)ConfigManager.LoadShardForFixingAssets);
        }

        public Harmony HarmonyInstance { get; private set; }
    }
}
