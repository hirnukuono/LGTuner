using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using LGTuner.Manager;
using LGTuner.Utils;

namespace LGTuner
{
    [BepInPlugin("LGTuner", "LGTuner", "1.0.6")]
    [BepInProcess("GTFO.exe")]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFOUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(MTFOPartialDataUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class EntryPoint : BasePlugin
    {
        public override void Load()
        {
            HarmonyInstance = new Harmony("LGTuner");
            HarmonyInstance.PatchAll();

            ConfigManager.Init();
        }

        public Harmony HarmonyInstance { get; private set; }
    }
}
