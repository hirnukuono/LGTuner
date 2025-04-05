using AssetShards;
using BepInEx.Unity.IL2CPP;
using Expedition;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using System;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Il2CppSystem.Collections.Hashtable;

namespace LGTuner.Inject
{
    [HarmonyPatch(typeof(LG_LoadComplexDataSetResourcesJob), nameof(LG_LoadComplexDataSetResourcesJob.Build))]
    internal static class Inject_LoadComplexShard
    {
        private static int _waitingShared = 0;

        private static void Prefix(LG_LoadComplexDataSetResourcesJob __instance)
        {
            if (__instance.m_loadingStarted)
                return;

            // fix assets
            if (!EntryPoint.AssetsFixed)
            {
                if (!IL2CPPChainloader.Instance.Plugins.TryGetValue("StairsFix", out var asdasdasdinfo))
                {
                    var go = AssetAPI.GetLoadedAsset<GameObject>("Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_Mining_Shaft.prefab");
                    var ladders = go.GetComponentsInChildren<LG_Ladder>();
                    ladders[2].gameObject.transform.position += new Vector3(0f, 0f, -1.5f);
                    Logger.Info("navmesh fix on Dimension_Desert_Mining_Shaft.prefab (StairsFix) done");
                }
                var go2 = AssetAPI.GetLoadedAsset<GameObject>("Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_SF_01.prefab");
                foreach (var tra in go2.GetComponentsInChildren<Transform>())
                {
                    if (tra.name == "AIGraphSource") tra.position = new(5, 2, -16);
                }
                EntryPoint.AssetsFixed = true;
                Logger.Info("navmesh fix on geo_64x64_service_floodways_hub_SF_01.prefab done");
            }

            foreach (var complex in BuilderInfo.ExtraComplexResourceToLoad)
            {
                var shardToLoad = complex switch
                {
                    Complex.Mining => AssetBundleName.Complex_Mining,
                    Complex.Service => AssetBundleName.Complex_Service,
                    Complex.Tech => AssetBundleName.Complex_Tech,
                    _ => AssetBundleName.None
                };

                if (shardToLoad != AssetBundleName.None)
                {
                    AssetShardManager.LoadAllShardsForBundleAsync(shardToLoad, new Action(Loaded), 1, LoadSceneMode.Additive);
                    _waitingShared++;
                }
            }
        }
        
        private static void Loaded()
        {
            _waitingShared--;
        }

        [HarmonyWrapSafe]
        private static void Postfix(LG_LoadComplexDataSetResourcesJob __instance, ref bool __result)
        {
            if (_waitingShared >= 1)
            {
                __result = false;
            }
        }
    }
}
