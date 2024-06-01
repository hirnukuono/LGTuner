using Expedition;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using LGTuner.Configs;
using LGTuner.Manager;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XXHashing;
using static LGTuner.Configs.ZoneOverrideData;

namespace LGTuner.Inject
{
    [HarmonyPatch(typeof(LG_LevelBuilder), nameof(LG_LevelBuilder.PlaceRoot))]
    internal static class Inject_BuildGeomorph
    {
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(LG_LevelBuilder.BuildFloor))]
        private static void Post_BuildFloor()
        {
            BuilderInfo.OnResourceSetSelected();
        }

        private static LayoutConfig _configContext = null;
        private static RotateType _nextRotateType;

        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(LG_LevelBuilder.PlaceRoot))]
        private static void Pre_PlaceRoot(LG_Tile tile, ref GameObject tilePrefab, ref bool forceAlignToVector, ref Vector3 alignVector, LG_Zone zone)
        {
            _configContext = null;
            _nextRotateType = RotateType.None;

            forceAlignToVector = alignVector != Vector3.zero; //forceAlignToVector is broken, It's needed to doing this

            var gridSize = zone.Dimension.GetGridSize();
            var normalPos = tile.ToNormalGrid(gridSize);
            Logger.Info($"tile info: {normalPos.x} {normalPos.z} {tilePrefab.name} for {zone.LocalIndex} : {zone.DimensionIndex}");

            if (!BuilderInfo.TryGetConfig(zone, out _configContext))
                return;
            
            if (!_configContext.TryGetTileData(normalPos, out var overrideData))
            {
                overrideData = _configContext.PopulateTileOverrideForZone(zone, normalPos);

                if (overrideData == null)
                    return;
            }

            if (!string.IsNullOrEmpty(overrideData.Geomorph))
            {
                var tempPrefab = AssetAPI.GetLoadedAsset(overrideData.Geomorph)?.Cast<GameObject>();
                if (tempPrefab != null)
                {
                    Logger.Info($" - tile overriden! {tempPrefab.name}");
                    tilePrefab = tempPrefab;
                }
            }

            if (overrideData.Rotation != RotateType.None)
            {
                _nextRotateType = overrideData.Rotation;
            }
        }

        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(LG_LevelBuilder.GetTilePosition))]
        private static void Post_GetTilePosition(LG_Tile tile, LG_Floor floor, int dimensionIndex, ref Vector3 __result)
        {
            if (_configContext == null)
                return;

            if (!floor.TryGetDimension(dimensionIndex, out var dimension))
                return;

            var gridSize = _configContext.GridSize;
            var normalGrid = tile.ToNormalGrid(gridSize);
            if (!_configContext.TryGetTileData(normalGrid, out var overrideData))
                return;

            if (overrideData.OverrideAltitude)
            {
                var position = dimension.Grid.GetPositionNormal(normalGrid.x, normalGrid.z, gridSize);
                position += new Vector3(0.0f, overrideData.Altitude * BuilderInfo.AltitudeOffset, 0.0f);
                __result = position;
            }
        }

        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(LG_LevelBuilder.PlaceRoot))]
        private static void Post_PlaceRoot(LG_Tile tile, LG_Zone zone, LG_Geomorph __result)
        {
            if (_configContext == null)
                return;

            if (_nextRotateType != RotateType.None)
            {
                var tileObject = __result.gameObject;
                var plugInfo = LG_PlugInfo.BuildPlugInfo(tileObject, tileObject.transform.rotation);

                Logger.Info($" - TRYING ROTATION! PLUG COUNT: {plugInfo.Count}");
                if (plugInfo.TryGetNewRotation(zone.m_subSeed, _nextRotateType, out var rotation))
                {
                    Logger.Info($" - Done!");
                    tileObject.transform.rotation = rotation;
                }

                __result.SetPlaced();
            }

            var gridSize = _configContext.GridSize;
            var normalGrid = tile.ToNormalGrid(gridSize);
            if (!_configContext.TryGetTileData(normalGrid, out var overrideData))
                return;

            if (overrideData.OverrideAreaSeeds)
            {
                if (overrideData.AreaSeeds.Length == __result.m_areas.Length)
                {
                    var length = __result.m_areas.Length;
                    for (int i = 0; i < length; i++)
                    {
                        var newSeed = overrideData.AreaSeeds[i];
                        if (newSeed != 0)
                        {
                            __result.m_areas[i].AreaSeed = newSeed;
                            Logger.Info($" - new area seed: {__result.m_areas[i].m_navInfo}, {newSeed}");
                        }
                    }
                }
                else
                {
                    Logger.Error($" - Area count and AreaSeeds item count mismatched! (CFG: {overrideData.AreaSeeds.Length} != AREA: {__result.m_areas.Length}) Area seed will not be applied!");
                }
            }
        }
    }
}