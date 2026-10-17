using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using LevelGeneration;
using LGTuner.Manager;
using LGTuner.Utils;
using System;
using System.Collections.Generic;

namespace LGTuner
{
    [BepInPlugin("LGTuner", "LGTuner", "1.2.8")]
    [BepInProcess("GTFO.exe")]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFOUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(MTFOPartialDataUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class EntryPoint : BasePlugin
    {
        public static bool StairsFixed = false;
        public static bool TechFixed = false;
        public static bool ServiceFixed = false;
        public static bool SamDBExitFixed = false;
        public static List<string> CustomMarkerPrefabs = new();

        public override void Load()
        {
            HarmonyInstance = new Harmony("LGTuner");
            HarmonyInstance.PatchAll();
            ConfigManager.Init();
        }

        public Harmony HarmonyInstance { get; private set; }
    }
}