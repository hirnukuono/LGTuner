using Expedition;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using LGTuner.Extensions;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace LGTuner.Inject
{
    [HarmonyPatch]
    internal static class Inject_BuildPlug
    {
        [HarmonyPatch(typeof(LG_BuildPlugBaseJob), nameof(LG_BuildPlugBaseJob.SpawnPlug))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyWrapSafe]
        private static void Pre_SpawnPlug(LG_Plug plug, ref GameObject prefab)
        {
            Logger.Debug($"found plug for zone: {plug.m_linksFrom.m_zone.LocalIndex}{plug.m_linksFrom.name}->{plug.m_linksTo.m_zone.LocalIndex}{plug.m_linksTo.name}");
            if (!BuilderInfo.TryGetConfig(plug.m_linksFrom.m_zone, out var config))
                return;
            var normalGrid = plug.m_linksFrom.m_geomorph.m_tile.m_shape.m_gridPosition.GetNormal(config.GridSize);
            if (!config.TryGetTileData(normalGrid, out var overrideData))
                return;
            if (overrideData.ZoneData?.OverridePlugs == true && TryGetPlugPrefab(overrideData.ZoneData.GetNextPlug(), out var newPrefab))
            {
                Logger.Info($" - Prefab Replaced by Zone OverridePlugs! {newPrefab.name}");
                prefab = newPrefab;
            }

            string prefabPath = plug.m_dir switch
            {
                LG_PlugDir.Up => overrideData.BackwardPlug,
                LG_PlugDir.Down => overrideData.ForwardPlug,
                LG_PlugDir.Left => overrideData.LeftPlug,
                LG_PlugDir.Right => overrideData.RightPlug,
                _ => string.Empty
            };
            if (TryGetPlugPrefab(prefabPath, out newPrefab))
            {
                Logger.Info($" - Prefab replaced by {plug.m_dir} plug setting! {newPrefab.name}");
                prefab = newPrefab;
                return;
            }

            if (!overrideData.OverridePlugWithNoGateChance)
                return;
            
            var rnd = new System.Random(plug.m_linksFrom.AreaSeed.ToSeed());
            bool hasGate;
            if (plug.m_isZoneSource)
            {
                hasGate = true;
            }
            else if (overrideData.PlugWithNoGateChance >= 1.0f)
            {
                hasGate = false;
            }
            else if (overrideData.PlugWithNoGateChance <= 0.0f)
            {
                hasGate = true;
            }
            else
            {
                hasGate = rnd.NextFloat01() >= overrideData.PlugWithNoGateChance;
            }

            int dropHeight = GetDropHeight(plug);
            var newPlug = BuilderInfo.GetRandomPlug(rnd.NextUInt(), dropHeight, GetSubComplexOfPlug(plug), hasGate);
            if (newPlug == null)
            {
                Logger.Error($"Plug was null! - Height: {dropHeight}");
                return;
            }
            prefab = newPlug;
        }

        [HarmonyPatch(typeof(LG_BuildPlugJob), nameof(LG_BuildPlugJob.Build))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyWrapSafe]
        private static bool Pre_BuildPlug(LG_BuildPlugJob __instance)
        {
            var plug = __instance.m_plug;
            Logger.Debug($"buildplugjob plug in complex {plug.m_subComplex} plug status {plug.ExpanderStatus} plug direction {plug.m_dir} ..");
            if (plug.ExpanderStatus == LG_ZoneExpanderStatus.Connected || plug.m_isZoneSource || !BuilderInfo.TryGetConfig(plug.m_linksFrom.m_zone, out var config))
                return true;
            var normalGrid = plug.m_linksFrom.m_geomorph.m_tile.m_shape.m_gridPosition.GetNormal(config.GridSize);
            if (!config.TryGetTileData(normalGrid, out var overrideData))
                return true;

            string prefabPath = plug.m_dir switch
            {
                LG_PlugDir.Up => overrideData.BackwardPlug,
                LG_PlugDir.Down => overrideData.ForwardPlug,
                LG_PlugDir.Left => overrideData.LeftPlug,
                LG_PlugDir.Right => overrideData.RightPlug,
                _ => string.Empty
            };

            if (TryGetPlugPrefab(prefabPath, out var prefab))
            {
                Logger.Info($" - Prefab replaced by {plug.m_dir} plug setting! {prefab.name}");
                var go = UnityEngine.Object.Instantiate(prefab, plug.transform.position, plug.transform.rotation);
                go.transform.SetParent(plug.transform, true);
                LG_Factory.FindAndBuildSelectorsAndSpawners(go, __instance.m_rnd.Random.NextSubSeed());
                __instance.ProcessDivider(plug, go, false, __instance.m_rnd.Random.NextSubSeed());
                plug.m_wasProcessed = true;
            }
            return true;
        }

        private static bool TryGetPlugPrefab(string prefabPath, [MaybeNullWhen(false)] out GameObject prefab)
        {
            if (string.IsNullOrEmpty(prefabPath))
            {
                prefab = null;
                return false;
            }
            prefab = AssetAPI.GetLoadedAsset<GameObject>(prefabPath);
            return prefab != null;
        }
        
        private static int GetDropHeight(LG_Plug plug)
        {
            float plugDistance = Mathf.Abs(plug.transform.position.y - plug.m_pariedWith.transform.position.y);
            return Mathf.RoundToInt(plugDistance / BuilderInfo.AltitudeOffset);
        }

        private static SubComplex GetSubComplexOfPlug(LG_Plug plug)
        {
            SubComplex subcomplex = plug.m_subComplex;
            if (plug.m_subComplex != plug.m_pariedWith.m_subComplex)
            {
                subcomplex = SubComplex.Plug_SubComplex_Transition;
            }
            return subcomplex;
        }        
    }
}
