using AssetShards;
using Expedition;
using HarmonyLib;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;

namespace LGTuner.Inject
{
    [HarmonyPatch(typeof(LG_LoadComplexDataSetResourcesJob), nameof(LG_LoadComplexDataSetResourcesJob.Build))]
    internal static class Inject_LoadComplexShard
    {
        private static int _waitingShared = 0;

        [HarmonyWrapSafe]
        private static void Prefix(LG_LoadComplexDataSetResourcesJob __instance)
        {
            if (__instance.m_loadingStarted)
                return;

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
