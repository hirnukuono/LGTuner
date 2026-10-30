using AmorLib.Dependencies;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using LGTuner.Manager;

namespace LGTuner
{
    [BepInPlugin(MODNAME, MODNAME, "1.3.0")]
    [BepInProcess("GTFO.exe")]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("Amor.AmorLib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFO_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PartialData_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    internal sealed class EntryPoint : BasePlugin
    {
        public const string MODNAME = "LGTuner";

        public override void Load()
        {
            Logger.MLS = Log;
            ConfigManager.Prepare(Config);
            new Harmony("LGTuner").PatchAll();
            ConfigManager.Init();
            Logger.Message("LGTuner is done loading!");
        }
    }
}
