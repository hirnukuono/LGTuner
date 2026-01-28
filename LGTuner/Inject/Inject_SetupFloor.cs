using AssetShards;
using Expedition;
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
        //private static LayoutConfig _configContext = null;

        [HarmonyPrefix]
        private static bool Prefix(LG_SetupFloor __instance, ref bool __result)
        {
            /// shardit ladattu

            foreach (var m in AssetShardManager.s_loadedManifests)
            {
                if (m.Key.Contains("Complex_Service") && !EntryPoint.ServiceFixed)
                {
                    var go2 = AssetAPI.GetLoadedAsset<GameObject>("Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_SF_01.prefab");
                    if (go2 != null)
                    {
                        foreach (var tra in go2.GetComponentsInChildren<Transform>())
                        {
                            if (tra.name == "AIGraphSource") tra.position = new(5, 2, -16);
                        }
                        Logger.Info("navmesh fix on geo_64x64_service_floodways_hub_SF_01.prefab done");
                        EntryPoint.ServiceFixed = true;
                    }
                }
                if (m.Key.Contains("Complex_Tech") && !EntryPoint.TechFixed)
                {
                    var go3 = AssetAPI.GetLoadedAsset<GameObject>("Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_HA_05.prefab");
                    if (go3 != null)
                    {
                        var caps = go3.GetComponentsInChildren<LG_PrefabSpawner>();
                        foreach (var c in caps)
                            if (c.transform.name.Contains("prop_generic_duct_d_2m_tile_001"))
                            {
                                var p = c.m_prefab;
                                foreach (var g in p.GetComponentsInChildren<Transform>())
                                    if (g.name == "Capsulecollider")
                                        g.transform.localScale = new(0.5f, 1, 0.5f);
                            }
                        Logger.Info("collider fix on geo_64x64_tech_lab_HA_05.prefab done");
                        EntryPoint.TechFixed = true;
                    }
                }
            }

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
            if (MainLayer.TileOverrides.Length == 0) return true;
            if (MainLayer.TileOverrides[0].X != 0 || MainLayer.TileOverrides[0].Z != 0) return true;

            if (!string.IsNullOrEmpty(MainLayer.TileOverrides[0].Geomorph))
            {
                if (EntryPoint.BundleLookup.ContainsKey(MainLayer.TileOverrides[0].Geomorph.ToUpperInvariant()) && !AssetShardManager.s_loadedAssetsLookup.ContainsKey(MainLayer.TileOverrides[0].Geomorph.ToUpperInvariant()))
                    foreach (var b in EntryPoint.BundleLookup)
                    {
                        if (b.Key == MainLayer.TileOverrides[0].Geomorph.ToUpperInvariant() && !EntryPoint.BundleLoadAllLookup.Contains(b.Value))
                        {
                            Logger.Info($"loading bundle asset prefab {MainLayer.TileOverrides[0].Geomorph.ToUpperInvariant()}");
                            UnityEngine.Object asset = b.Value.LoadAsset(MainLayer.TileOverrides[0].Geomorph.ToUpperInvariant());
                            try { AssetShardManager.s_loadedAssetsLookup.Add(MainLayer.TileOverrides[0].Geomorph.ToUpperInvariant(), asset); } catch { }
                        }
                        if (b.Key == MainLayer.TileOverrides[0].Geomorph.ToUpperInvariant() && EntryPoint.BundleLoadAllLookup.Contains(b.Value))
                        {
                            Logger.Info($"loading all assets from bundle {b.Value.name}");
                            foreach (var a in b.Value.GetAllAssetNames())
                            {
                                UnityEngine.Object asset = b.Value.LoadAsset(a);
                                try { AssetShardManager.s_loadedAssetsLookup.Add(a.ToUpperInvariant(), asset); } catch { }
                            }
                        }
                    }


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