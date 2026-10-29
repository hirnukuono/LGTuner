using BepInEx;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using LGTuner.Manager;
using UnityEngine;
using XXHashing;

namespace LGTuner.Inject
{
    [HarmonyPatch(typeof(LG_SetupFloor), nameof(LG_SetupFloor.Build))]
    internal static class Inject_SetupFloor
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        private static bool Pre_LG_SetupFloor_Build(LG_SetupFloor __instance, ref bool __result)
        {
            Debug.Log(Deb.LG("LG_SetupFloor.Build"));
            uint x = 0u;
            if (Builder.LayerBuildDatas[0].m_zoneBuildDatas != null && Builder.LayerBuildDatas[0].m_zoneBuildDatas.Count > 0)
            {
                x = (uint)Builder.LayerBuildDatas[0].m_zoneBuildDatas[0].SubSeed;
            }
            XXHashSequence xXHashSequence = new(__instance.m_rnd.Seed.SubSeed(x));            
            if (!ConfigManager.TryGetConfig(RundownManager.ActiveExpedition.LevelLayoutData, out var config) || config.TileOverrides.Length == 0)
                return true;
            var firstTileOverride = config.TileOverrides.OrderBy(t => t.X).ThenBy(t => t.Z).First();
            if (firstTileOverride.X != 0 || firstTileOverride.Z != 0 || firstTileOverride.Geomorph.IsNullOrWhiteSpace())
                return true;
            
            var go = AssetAPI.GetLoadedAsset<GameObject>(firstTileOverride.Geomorph);
            if (go == null)
            {
                Logger.Error("LGTuner failed to override elevator!");
                return true;
            }
            Logger.Info($" - elevator overriden! {go.name}");
            var floorTransition = GOUtil.SpawnChildAndGetComp<LG_FloorTransition>(go, __instance.m_position, __instance.m_rotation);
            floorTransition.m_geoPrefab = go;
            floorTransition.SetupAreas(xXHashSequence.NextSubSeed());
            floorTransition.SetPlaced();
            __instance.m_floor.Setup(xXHashSequence.NextSubSeed(), LG_FloorType.StartFloor, Builder.ComplexResourceSetBlock.LevelGenConfig.TransitionDirection, floorTransition);
            for (int i = 0; i < __instance.m_floor.m_dimensions.Count; i++)
            {
                __instance.m_floor.InjectJobs(__instance.m_floor.m_dimensions[i].DimensionIndex);
            }
            LG_Factory.InjectJob(new LG_BuildFloorJob(__instance.m_floor), LG_Factory.BatchName.BuildFloors);
            __result = true;
            return false;            
        }
    }
}