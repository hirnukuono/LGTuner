using Expedition;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace LGTuner.Inject
{
    [HarmonyPatch(typeof(LG_BuildPlugBaseJob), nameof(LG_BuildPlugBaseJob.SpawnPlug))]
    internal static class Inject_BuildPlug
    {
        private static void Prefix(LG_Plug plug, ref GameObject prefab)
        {
            Logger.Info($"found plug for zone: {plug.m_linksFrom.m_zone.LocalIndex}{plug.m_linksFrom.name}->{plug.m_linksTo.m_zone.LocalIndex}{plug.m_linksTo.name}");

            if (!BuilderInfo.TryGetConfig(plug.m_linksFrom.m_zone, out var config))
                return;

            var normalGrid = plug.m_linksFrom.m_geomorph.m_tile.m_shape.m_gridPosition.GetNormal(config.GridSize);
            if (!config.TryGetTileData(normalGrid, out var overrideData))
                return;

            if (overrideData.ZoneData?.OverridePlugs ?? false)
            {
                if (TryGetPlugPrefab(overrideData.ZoneData.GetNextPlug(), out var newPrefab))
                {
                    Logger.Info($" - Prefab Replaced by Zone OverridePlugs! {newPrefab.name}");
                    prefab = newPrefab;
                }
            }

            switch (plug.m_dir)
            {
                case LG_PlugDir.Up:
                    if (TryGetPlugPrefab(overrideData.BackwardPlug, out var newPrefab))
                    {
                        Logger.Info($" - Prefab Replaced by BackwardPlug setting! {newPrefab.name}");
                        prefab = newPrefab;
                        return;
                    }
                    break;

                case LG_PlugDir.Down:
                    if (TryGetPlugPrefab(overrideData.ForwardPlug, out newPrefab))
                    {
                        Logger.Info($" - Prefab Replaced by ForwardPlug setting! {newPrefab.name}");
                        prefab = newPrefab;
                        return;
                    }
                    break;

                case LG_PlugDir.Left:
                    if (TryGetPlugPrefab(overrideData.LeftPlug, out newPrefab))
                    {
                        Logger.Info($" - Prefab Replaced by LeftPlug setting! {newPrefab.name}");
                        prefab = newPrefab;
                        return;
                    }
                    break;

                case LG_PlugDir.Right:
                    if (TryGetPlugPrefab(overrideData.RightPlug, out newPrefab))
                    {
                        Logger.Info($" - Prefab Replaced by RightPlug setting! {newPrefab.name}");
                        prefab = newPrefab;
                        return;
                    }
                    break;
            }

            if (overrideData.OverridePlugWithNoGateChance)
            {
                var rnd = new Random(plug.m_linksFrom.AreaSeed.ToSeed());

                bool hasGate;
                if (plug.m_isZoneSource)
                {
                    hasGate = true;
                }
                else
                {
                    if (overrideData.PlugWithNoGateChance >= 1.0f)
                    {
                        hasGate = false;
                    }
                    else if (overrideData.PlugWithNoGateChance <= 0.0f)
                    {
                        hasGate = true;
                    }
                    else
                    {
                        hasGate = rnd.NextFloat() >= overrideData.PlugWithNoGateChance;
                    }
                }

                var dropHeight = GetDropHeight(plug);
                var newPlug = BuilderInfo.GetRandomPlug(rnd.NextUint(), dropHeight, GetSubComplexOfPlug(plug), hasGate);
                if (newPlug == null)
                {
                    Logger.Error($"Plug was null! - Height: {dropHeight}");
                    return;
                }

                prefab = newPlug;
            }
        }

        private static SubComplex GetSubComplexOfPlug(LG_Plug plug)
        {
            var subcomplex = plug.m_subComplex;
            if (plug.m_subComplex != plug.m_pariedWith.m_subComplex)
            {
                subcomplex = SubComplex.Plug_SubComplex_Transition;
            }
            return subcomplex;
        }

        private static bool TryGetPlugPrefab(string prefabPath, out GameObject prefab)
        {
            if (string.IsNullOrEmpty(prefabPath))
            {
                prefab = null;
                return false;
            }

            prefab = AssetAPI.GetLoadedAsset(prefabPath).Cast<GameObject>();
            return prefab != null;
        }

        private static int GetDropHeight(LG_Plug plug)
        {
            var plugDistance = Mathf.Abs(plug.transform.position.y - plug.m_pariedWith.transform.position.y);
            return Mathf.RoundToInt(plugDistance / BuilderInfo.AltitudeOffset);
        }
    }
}
