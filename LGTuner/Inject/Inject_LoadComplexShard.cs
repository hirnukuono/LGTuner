using AssetShards;
using Expedition;
using HarmonyLib;
using LevelGeneration;
using UnityEngine.SceneManagement;

namespace LGTuner.Inject
{
    [HarmonyPatch(typeof(LG_LoadComplexDataSetResourcesJob), nameof(LG_LoadComplexDataSetResourcesJob.Build))]
    internal static class Inject_LoadComplexShard
    {
        private static int _waitingShared = 0;
        private static void Loaded() => _waitingShared--;

        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyWrapSafe]
        private static void Pre_LoadComplexShard_Build(LG_LoadComplexDataSetResourcesJob __instance)
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

        [HarmonyPostfix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyWrapSafe]
        private static void Post_LoadComplexShard_Build(ref bool __result)
        {
            if (_waitingShared >= 1)
                __result = false;            
        }
    }
}
