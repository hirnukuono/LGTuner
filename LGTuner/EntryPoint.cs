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
    [BepInDependency(MTFO_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PartialData_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    internal sealed class EntryPoint : BasePlugin
    {
        public const string MODNAME = "LGTuner";

        public override void Load()
        {
            new Harmony("LGTuner").PatchAll();
            ConfigManager.Init();
            ConfigManager.Prepare(Config);
            Logger.Info("LGTuner is done loading!");
        }
    }
}
// todo - refactor for patch classes
// todo - move geo fixes to own holder
// todo - custom converter for layout config to read as json array or object