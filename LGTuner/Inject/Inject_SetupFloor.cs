using Expedition;
using GameData;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using LevelGeneration.Core;
using LGTuner.Configs;
using LGTuner.Manager;
using UnityEngine;
using XXHashing;

namespace LGTuner.Inject
{
    [HarmonyPatch(typeof(LG_SetupFloor), nameof(LG_SetupFloor.Build))]
    internal static class Inject_SetupFloor
    {
        private static LayoutConfig _configContext = null;

        [HarmonyPrefix]
        private static bool Prefix(LG_SetupFloor __instance, ref bool __result)
        {
            LG_Zone zone = null;
            LayoutConfig MainLayer = null;
            Debug.Log(Deb.LG("LG_SetupFloor.Build"));
            GameObject gameObject = null;
            SubComplex subcomplex = SubComplex.All;
            uint x = 0u;
            if (Builder.LayerBuildDatas[0].m_zoneBuildDatas != null && Builder.LayerBuildDatas[0].m_zoneBuildDatas.Count > 0)
            {
                subcomplex = Builder.LayerBuildDatas[0].m_zoneBuildDatas[0].SubComplex;
                x = (uint)Builder.LayerBuildDatas[0].m_zoneBuildDatas[0].SubSeed;
            }

            XXHashSequence xXHashSequence = new XXHashSequence(__instance.m_rnd.Seed.SubSeed(x));

            var activeExp = RundownManager.ActiveExpedition;
            ConfigManager.TryGetConfig(activeExp.LevelLayoutData, out MainLayer);
            if (MainLayer == null) return true;

            LG_GridPosition normalPos = new() { x = 0, z = 0 };
            if (MainLayer != null)
            {
                if (MainLayer.TileOverrides[0].X != 0 || MainLayer.TileOverrides[0].Z != 0) return true;
            }

            if (!string.IsNullOrEmpty(MainLayer.TileOverrides[0].Geomorph))
            {
                gameObject = AssetAPI.GetLoadedAsset(MainLayer.TileOverrides[0].Geomorph)?.Cast<GameObject>();

                if (gameObject == null)
                {
                    Logger.Info("LGTuner failed to override elevator!");
                    return true;
                }

                Logger.Info($" - elevator overriden! {gameObject.name}");

                LG_FloorTransition lG_FloorTransition = GOUtil.SpawnChildAndGetComp<LG_FloorTransition>(gameObject, __instance.m_position, __instance.m_rotation);
                lG_FloorTransition.m_geoPrefab = gameObject;
                lG_FloorTransition.SetupAreas(xXHashSequence.NextSubSeed());
                lG_FloorTransition.SetPlaced();
                __instance.m_floor.Setup(xXHashSequence.NextSubSeed(), LG_FloorType.StartFloor, Builder.ComplexResourceSetBlock.LevelGenConfig.TransitionDirection, lG_FloorTransition);
                for (int i = 0; i < __instance.m_floor.m_dimensions.Count; i++)
                {
                    __instance.m_floor.InjectJobs(__instance.m_floor.m_dimensions[i].DimensionIndex);
                }
                LG_Factory.InjectJob(new LG_BuildFloorJob(__instance.m_floor), LG_Factory.BatchName.BuildFloors);
                __result = true;
                return false;
            }
            return true;
        }
    }
}