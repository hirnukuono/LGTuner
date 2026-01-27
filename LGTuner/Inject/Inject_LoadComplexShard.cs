using AssetShards;
using BepInEx.Unity.IL2CPP;
using Expedition;
using GameData;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using MultiplayerBasicExample;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            if (!IL2CPPChainloader.Instance.Plugins.TryGetValue("StairsFix", out var asdasdasdinfo) && !EntryPoint.StairsFixed)
            {
                var go = AssetAPI.GetLoadedAsset<GameObject>("Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_Mining_Shaft.prefab");
                var ladders = go.GetComponentsInChildren<LG_Ladder>();
                ladders[2].gameObject.transform.position += new Vector3(0f, 0f, -1.5f);
                Logger.Info("navmesh fix on Dimension_Desert_Mining_Shaft.prefab (StairsFix) done");
                EntryPoint.StairsFixed = true;
            }

            /// find out if complexresourceset and marker datablocks contain funny bundle assets
            List<string> prefabstoload = new();
            List<AssetBundle> loadedbundles = new();
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).CustomGeomorphs_Challenge_1x1)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).CustomGeomorphs_Exit_1x1)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).CustomGeomorphs_Objective_1x1)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToString().ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).GeomorphTiles_1x1)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).GeomorphTiles_2x1)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).GeomorphTiles_2x2)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).ElevatorShafts_1x1)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).PlugCaps)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).DoubleDropPlugsNoGates)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).SingleDropPlugsNoGates)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).StraightPlugsNoGates)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).DoubleDropPlugsWithGates)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).SingleDropPlugsWithGates)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());
            foreach (var g in ComplexResourceSetDataBlock.GetBlock(RundownManager.ActiveExpedition.Expedition.ComplexResourceData).StraightPlugsWithGates)
                if (!prefabstoload.Contains(g.Prefab.ToUpperInvariant())) prefabstoload.Add(g.Prefab.ToUpperInvariant());

            foreach (var b in MiningMarkerDataBlock.GetAllBlocks())
                if (b != null)
                    if (b.CommonData != null)
                        foreach (var c in b.CommonData.Compositions)
                            if (c != null)
                                if (c.prefab != null)
                                    if (EntryPoint.BundleLookup.ContainsKey(c.prefab.ToUpperInvariant()) && !AssetShardManager.s_loadedAssetsLookup.ContainsKey(c.prefab.ToUpperInvariant()))
                                    {
                                        UnityEngine.Object asset = EntryPoint.BundleLookup[c.prefab.ToUpperInvariant()].LoadAsset(c.prefab.ToUpperInvariant());
                                        try { AssetShardManager.s_loadedAssetsLookup.Add(c.prefab.ToUpperInvariant(), asset); } catch { }
                                        EntryPoint.CustomMarkerPrefabs.Add(c.prefab.ToUpperInvariant());
                                    }

            foreach (var b in TechMarkerDataBlock.GetAllBlocks())
                if (b != null)
                    if (b.CommonData != null)
                        foreach (var c in b.CommonData.Compositions)
                            if (c != null)
                                if (c.prefab != null)
                                    if (EntryPoint.BundleLookup.ContainsKey(c.prefab.ToUpperInvariant()) && !AssetShardManager.s_loadedAssetsLookup.ContainsKey(c.prefab.ToUpperInvariant()))
                                    {
                                        UnityEngine.Object asset = EntryPoint.BundleLookup[c.prefab.ToUpperInvariant()].LoadAsset(c.prefab.ToUpperInvariant());
                                        try { AssetShardManager.s_loadedAssetsLookup.Add(c.prefab.ToUpperInvariant(), asset); } catch { }
                                        EntryPoint.CustomMarkerPrefabs.Add(c.prefab.ToUpperInvariant());

                                    }

            foreach (var b in ServiceMarkerDataBlock.GetAllBlocks())
                if (b != null)
                    if (b.CommonData != null)
                        foreach (var c in b.CommonData.Compositions)
                            if (c != null)
                                if (c.prefab != null)
                                    if (EntryPoint.BundleLookup.ContainsKey(c.prefab.ToUpperInvariant()) && !AssetShardManager.s_loadedAssetsLookup.ContainsKey(c.prefab.ToUpperInvariant()))
                                    {
                                        UnityEngine.Object asset = EntryPoint.BundleLookup[c.prefab.ToUpperInvariant()].LoadAsset(c.prefab.ToUpperInvariant());
                                        try { AssetShardManager.s_loadedAssetsLookup.Add(c.prefab.ToUpperInvariant(), asset); } catch { }
                                        EntryPoint.CustomMarkerPrefabs.Add(c.prefab.ToUpperInvariant());
                                    }

            foreach (var g in prefabstoload)
                if (EntryPoint.BundleLookup.ContainsKey(g) && !AssetShardManager.s_loadedAssetsLookup.ContainsKey(g))
                    foreach (var temp in EntryPoint.BundleLookup)
                    {
                        if (temp.Key == g && !EntryPoint.BundleLoadAllLookup.Contains(temp.Value))
                        {
                            Logger.Info($"loading bundle asset prefab {g} ..");
                            UnityEngine.Object asset = temp.Value.LoadAsset(g);
                            try { AssetShardManager.s_loadedAssetsLookup.Add(g.ToUpperInvariant(), asset); } catch { }
                        }
                        if (temp.Key == g && EntryPoint.BundleLoadAllLookup.Contains(temp.Value))
                        {
                            Logger.Info($"loading all assets from bundle {temp.Value.name}");
                            foreach (var a in temp.Value.GetAllAssetNames())
                            {
                                UnityEngine.Object asset = temp.Value.LoadAsset(a);
                                try { AssetShardManager.s_loadedAssetsLookup.Add(a.ToUpperInvariant(), asset); } catch { }
                            }
                        }
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
