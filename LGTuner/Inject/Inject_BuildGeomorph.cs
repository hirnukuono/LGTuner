using BepInEx;
using BepInEx.Logging;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using LGTuner.Configs;
using UnityEngine;

namespace LGTuner.Inject
{
    [HarmonyPatch]
    internal static class Inject_BuildGeomorph
    {
        private static LayoutConfig _configContext = null!;
        private static LayoutConfig _configContext2 = null!;
        private static RotateType _nextRotateType;

        [HarmonyPatch(typeof(LG_LevelBuilder), nameof(LG_LevelBuilder.PlaceRoot))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyWrapSafe]
        private static void Pre_PlaceRoot(LG_Tile tile, ref GameObject tilePrefab, ref bool forceAlignToVector, ref Vector3 alignVector, LG_Zone zone)
        {
            _configContext = null!;
            _nextRotateType = RotateType.None;
            forceAlignToVector = alignVector != Vector3.zero; // forceAlignToVector is broken, it's necessary to do 
            var gridSize = zone.Dimension.GetGridSize();
            var normalPos = tile.ToNormalGrid(gridSize);
            Logger.Verbose($"tile info: {normalPos.x} {normalPos.z} {tilePrefab.name} for {zone.LocalIndex} : {zone.DimensionIndex}");

            if (!BuilderInfo.TryGetConfig(zone, out _configContext))
                return;
            if (!_configContext.TryGetTileData(normalPos, out var overrideData))
                overrideData = _configContext.PopulateTileOverrideForZone(zone, normalPos);
            if (overrideData == null)
                return;

            if (!overrideData.Geomorph.IsNullOrWhiteSpace())
            {
                if (overrideData.X == 0 && overrideData.Z == 0) 
                    return;

                var tempPrefab = AssetAPI.GetLoadedAsset<GameObject>(overrideData.Geomorph);
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

        [HarmonyPatch(typeof(LG_LevelBuilder), nameof(LG_LevelBuilder.PlaceRoot))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyWrapSafe]
        private static void Post_PlaceRoot(LG_Tile tile, LG_Zone zone, LG_Geomorph __result)
        {
            if (_configContext == null)
                return;

            if (_nextRotateType != RotateType.None)
            {
                var tileObject = __result.gameObject;
                var plugInfo = LG_PlugInfo.BuildPlugInfo(tileObject, tileObject.transform.rotation);
                Logger.Verbose($" - TRYING ROTATION! PLUG COUNT: {plugInfo.Count}", LogLevel.Message);
                if (plugInfo.TryGetNewRotation(zone.m_subSeed, _nextRotateType, out var rotation))
                {
                    Logger.Verbose(" - Done!", LogLevel.Info);
                    tileObject.transform.rotation = rotation;
                }
                __result.SetPlaced();
            }

            if (!_configContext.TryGetTileData(tile.ToNormalGrid(_configContext.GridSize), out var overrideData))
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
                            Logger.Debug($" - new area seed: {__result.m_areas[i].m_navInfo}, {newSeed}");
                        }
                    }
                }
                else
                {
                    Logger.Error($" - Area count and AreaSeeds item count mismatched! (CFG: {overrideData.AreaSeeds.Length} != AREA: {__result.m_areas.Length}) Area seed will not be applied!");
                }
            }

            // the following gets run on its own if there's a LevelLayoutDB Custom Geo
            if (!zone.m_settings.HasCustomGeomorphPrefab)
            {
                var components = __result.GetComponentsInChildren<iLG_CustomGeomorphLogic>();
                for (int i = 0; i < components.Length; i++)
                {
                    components[i].SpawnedOnFloor();
                    LG_Factory.InjectJob(new LG_CustomGeomorphBuildJob(components[i]), LG_Factory.BatchName.CustomGeomorphBuildJob);
                    LG_Factory.InjectJob(new LG_CustomGeomorphPostCullingJob(components[i]), LG_Factory.BatchName.CustomGeomorphPostCullingJob);
                }
            }
        }

        [HarmonyPatch(typeof(LG_LevelBuilder), nameof(LG_LevelBuilder.BuildFloor))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        private static void Post_BuildFloor()
        {
            BuilderInfo.OnResourceSetSelected();
        }

        [HarmonyPatch(typeof(LG_LevelBuilder), nameof(LG_LevelBuilder.GetTilePosition))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyWrapSafe]
        private static void Post_GetTilePosition(LG_Tile tile, LG_Floor floor, int dimensionIndex, ref Vector3 __result)
        {
            if (_configContext == null || !floor.TryGetDimension(dimensionIndex, out var dimension))
                return;

            var gridSize = _configContext.GridSize;
            var normalGrid = tile.ToNormalGrid(gridSize);
            if (!_configContext.TryGetTileData(normalGrid, out var overrideData))
                return;

            if (overrideData.OverrideAltitude)
            {
                Vector3 position = dimension.Grid.GetPositionNormal(normalGrid.x, normalGrid.z, gridSize);
                position += new Vector3(0.0f, overrideData.Altitude * BuilderInfo.AltitudeOffset, 0.0f);
                __result = position;
            }
        }

        [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        private static void Pre_ZoneJob(LG_ZoneJob_CreateExpandFromData __instance)
        {
            if (Builder.CurrentFloor == null || !__instance.m_zone.m_settings.HasCustomGeomorphInstance || __instance.m_mainStatus != LG_ZoneJob_CreateExpandFromData.MainStatus.Done) 
                return;
            
            TileOverrideData? overrideData = null;
            if (BuilderInfo.TryGetConfig(__instance.m_zone, out _configContext2))
            {
                var gridSize = _configContext2.GridSize;
                var normalGrid = __instance.m_zone.m_settings.CustomGeomorphInstance.GetComponent<LG_Geomorph>().m_tile.ToNormalGrid(gridSize);
                _configContext2.TryGetTileData(normalGrid, out overrideData);
            }
            if (overrideData == null || !overrideData.TryForceCustomGeoAreaAssignment) 
                return;

            Logger.Info($" - addcustomgeomorphareas for {__instance.m_zone.NavInfo.ToString()}");
            Dictionary<LG_Area, float> listdict = new();
            foreach (var c in __instance.m_zone.m_areas)
            {
                float score = 1000;
                if (c.m_zone.ID == 0 && c == c.m_zone.m_areas[0]) 
                    score = 2000;
                if (c == c.m_zone.m_sourceExpander.m_linksTo) 
                    score = 1500;
                listdict.Add(c, score - Vector3.Distance(c.Position, __instance.m_zone.m_sourceExpander.m_linksFrom.Position));
            }

            var finalList = listdict.OrderByDescending(listdict => listdict.Value);
            __instance.m_zone.m_areas.Clear();
            foreach (var kvp in finalList)
            {
                Logger.Verbose($" - addcustomgeomorphareas scoredict {kvp.Key.name} {kvp.Value}");
                if (!__instance.m_zone.m_areas.Contains(kvp.Key)) 
                    __instance.m_zone.m_areas.Add(kvp.Key);

                foreach (var internalGate in kvp.Key.gameObject.GetComponentsInChildren<LG_InternalGate>(true))
                {
                    if (internalGate.m_linksFrom != null && !__instance.m_zone.m_areas.Contains(internalGate.m_linksFrom))
                    {
                        Logger.Verbose($" - addcustomgeomorphareas adding m_linksFrom {internalGate.m_linksFrom.name}");
                        __instance.m_zone.m_areas.Add(internalGate.m_linksFrom);
                    }
                    if (internalGate.m_linksTo != null && !__instance.m_zone.m_areas.Contains(internalGate.m_linksTo))
                    {
                        Logger.Verbose($" - addcustomgeomorphareas adding m_linksTo {internalGate.m_linksTo.name}");
                        __instance.m_zone.m_areas.Add(internalGate.m_linksTo);
                    }
                }
            }
        }
    }
}
