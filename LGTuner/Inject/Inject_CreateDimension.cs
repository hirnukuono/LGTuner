using AIGraph;
using AmorLib.Utils;
using BepInEx;
using GameData;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using LevelGeneration.Core;
using LGTuner.Manager;
using LogUtils;
using SNetwork;
using UnityEngine;
using XXHashing;

namespace LGTuner.Inject
{
    [HarmonyPatch]
    internal static class Inject_CreateDimension
    {
        [HarmonyPatch(typeof(LG_Floor), nameof(LG_Floor.CreateAllDimensions))]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        private static bool Pre_CreateAllDim250(LG_Floor __instance, uint seed, bool skipMainDimension = true)
        {
            int gridSize = 40;
            if (Builder.LevelGenExpedition.DimensionDatas == null || Builder.LevelGenExpedition.DimensionDatas.Count <= 0)
            {
                Dimension.CalculateAllDimensionBounds();
                return false;
            }

            XXHashSequence xXHashSequence = new(seed);
            int num = 0;
            float num2 = 150f;
            for (int i = 0; i < Builder.LevelGenExpedition.DimensionDatas.Count; i++)
            {
                var dimExpData = Builder.LevelGenExpedition.DimensionDatas[i];
                if (dimExpData.DimensionIndex != eDimensionIndex.ARENA_DIMENSION && dimExpData.TryGetDimensionData(out var dimBlock))
                {
                    var dimData = dimBlock.DimensionData;
                    if (dimExpData.Enabled && (!skipMainDimension || dimExpData.DimensionIndex != eDimensionIndex.Reality) && !__instance.m_createdDimensionSet.Contains(dimExpData.DimensionIndex))
                    {
                        num2 += dimData.VerticalExtentsDown;
                        Vector3 position = Vector3.up * num2;
                        __instance.CreateDimension(xXHashSequence.NextSubSeed(), dimExpData.DimensionIndex, false, dimData, position, gridSize, 64f);
                        __instance.m_createdDimensionSet.Add(dimExpData.DimensionIndex);
                        Logger.Debug($"created dim {dimExpData.DimensionIndex} pos {position} ..");
                        num2 += dimData.VerticalExtentsUp; // removed +50, eeeediot hirnu
                        num++;
                    }
                }
            }

            float num3 = 0f;
            float num4 = 0f;
            for (int j = 0; j < Builder.LevelGenExpedition.DimensionDatas.Count; j++)
            {
                var dimExpData2 = Builder.LevelGenExpedition.DimensionDatas[j];
                if (dimExpData2.DimensionIndex == eDimensionIndex.ARENA_DIMENSION && dimExpData2.TryGetDimensionData(out var dimBlock2))
                {
                    if (dimBlock2.DimensionData.VerticalExtentsDown > num3)
                    {
                        num3 = dimBlock2.DimensionData.VerticalExtentsDown;
                    }
                    if (dimBlock2.DimensionData.VerticalExtentsUp > num3)
                    {
                        num4 = num3;
                    }
                }
            }

            int num5 = 0;
            for (int k = 0; k < SNet.Slots.PlayerSlots.Length; k++)
            {
                num2 += num3;
                float num6 = 0f;
                for (int l = 0; l < Builder.LevelGenExpedition.DimensionDatas.Count; l++)
                {
                    var dimExpData3 = Builder.LevelGenExpedition.DimensionDatas[l];
                    if (dimExpData3.DimensionIndex != eDimensionIndex.ARENA_DIMENSION)
                    {
                        continue;
                    }
                    if (!dimExpData3.TryGetDimensionData(out var dimBlock3))
                    {
                        Debug.LogError("Could not find datablock for arena: datablockID: " + dimExpData3.DimensionData);
                        continue;
                    }
                    eDimensionIndex fDimensionIndex = (eDimensionIndex)(21 - (num5 + 1));
                    if (__instance.m_createdDimensionSet.Contains(fDimensionIndex))
                    {
                        DebugLog.LogPrefixError(__instance, $"Enemy dimension ({fDimensionIndex}) had already been created! If there are dimension enemies in the level, dimensionIndex {eDimensionIndex.Dimension_17}-{eDimensionIndex.Dimension_20} needs to be free (one for each player slot).");
                        continue;
                    }

                    var dimData2 = dimBlock3.DimensionData;
                    if (dimExpData3.Enabled)
                    {
                        var customGeomorph = GameDataBlockBase<ComplexResourceSetDataBlock>.GetBlock(dimData2.DimensionResourceSetID).GetCustomGeomorph(dimData2.DimensionGeomorph);
                        if (customGeomorph == null)
                        {
                            DebugLog.LogPrefixError(__instance, $"The dimension geomorph for ({dimBlock3.persistentID}) {dimBlock3.name} could not be found!");
                            continue;
                        }
                        float num7 = 32f;
                        var dimensionLevel = customGeomorph.GetComponent<LG_Dimension>();
                        if (dimensionLevel != null && dimensionLevel.TryGetDimensionBounds(out var bounds))
                        {
                            num7 += bounds.extents.x;
                        }
                        num6 += num7;
                        Vector3 position2 = Vector3.up * num2 + Vector3.right * num6;
                        __instance.CreateDimension(xXHashSequence.NextSubSeed(), fDimensionIndex, true, dimData2, position2, 10, 64f);
                        __instance.m_createdDimensionSet.Add(fDimensionIndex);
                        DimensionSlotIndex dimSlotIndex = new() { DimensionDataBlockID = 14, PlayerSlotIndex = (uint)k };
                        __instance.m_dimensionArenaMap.Add(dimSlotIndex, __instance.m_dimensions[^1]);
                        Logger.Debug($"created dim {fDimensionIndex} pos {position2} ..");
                        num++;
                        num6 += num7;
                    }
                    num5++;
                }
                num2 += num4;
            }

            Dimension.CalculateAllDimensionBounds();
            Logger.Info($"Found ({num}) unique dimension references in GameData for this expedition and created ({num5}) enemy dimensions.");
            return false;
        }

        [HarmonyPatch(typeof(LG_Floor), nameof(LG_Floor.CreateDimension))]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        private static bool Pre_CreateDimension(LG_Floor __instance, uint seed, eDimensionIndex dimensionIndex, bool arenaDimension, ref DimensionData dimensionData, Vector3 position, ref int __result, int gridSize, float cellSize = 64f)
        {
            if (arenaDimension) 
                return true;            
            gridSize = 40;
            if (dimensionData.IsStaticDimension) 
                return true;
            if (!DataBlockUtil.TryGetBlock<LevelLayoutDataBlock>(dimensionData.LevelLayoutData, out var layoutBlock) || !ConfigManager.TryGetConfig(layoutBlock.persistentID, out var config) || config.TileOverrides.Length == 0) 
                return true;

            Logger.Debug($"in createdim prefix gridsize {gridSize} ..");
            cellSize = 64f;
            XXHashSequence xXHashSequence = new(seed);
            Vector3 gridPosition = position - new Vector3(cellSize, 0f, cellSize) * gridSize * 0.5f;
            var tileShape = __instance.CreateTileShape(LG_TileShapeType.s1x1, cellSize, ref gridPosition);
            var grid = CellGridBase<LG_Grid, LG_Tile, LG_Cell>.Create(cellSize, gridPosition);
            grid.SetupCells(gridSize, gridSize);
            grid.InititalizeCells();
            int count = __instance.m_dimensions.Count;
            Dimension dimension = new(grid, dimensionIndex, count, dimensionData, position, arenaDimension);
            DebugLog.LogPrefix(__instance, $"Created Dimension (Index: {dimensionIndex}, gridSize: {gridSize}, cellSize: {cellSize}).");
            var customGeomorph = dimension.ResourceData.GetCustomGeomorph(dimensionData.DimensionGeomorph);
            var firstTileOverride = config.TileOverrides.OrderBy(t => t.X).ThenBy(t => t.Z).First();

            if (firstTileOverride.X == 0 && firstTileOverride.Z == 0 && !firstTileOverride.Geomorph.IsNullOrWhiteSpace())
            {
                customGeomorph = AssetAPI.GetLoadedAsset<GameObject>(firstTileOverride.Geomorph);
                Logger.Info($" - dim {dimensionIndex} elevator overriden! {customGeomorph.name}");
            }
            if (customGeomorph == null)
            {
                DebugLog.LogPrefixError(__instance, $"<b><color=red>NULL ERROR</color></b> Could not get the Dimension Geomorph prefab for dimension {dimensionIndex}, are you using a resource set in the DimensionDataBlock that contains the Dimension Geomorph prefab under the \"Custom Geomorphs_Objective_1x1\" header?");
                __result = count;
                return false;
            }
            var floorTransition = __instance.CreateFloorTransition(xXHashSequence.NextSubSeed(), dimension, position, Quaternion.identity, customGeomorph, dimension.DimensionData.IsStaticDimension);
            if (floorTransition == null)
            {
                DebugLog.LogPrefixError(__instance, $"<b><color=red>NULL ERROR</color></b> The LG_FloorTransition of dimension {dimensionIndex} was null! Does the dimension prefab have a LG_FloorTransition component on its root?");
                __result = count;
                return false;
            }
            var dimensionLevel = floorTransition.GetComponent<LG_Dimension>();
            if (dimensionLevel == null)
            {
                dimensionLevel = floorTransition.gameObject.AddComponent<LG_Dimension>();
                DebugLog.LogPrefixError(__instance, $"<b><color=red>NULL ERROR</color></b> The dimension {dimensionIndex} had no {typeof(LG_Dimension).Name} component, so added one automatically!");
            }
            dimension.DimensionLevel = dimensionLevel;
            if (floorTransition.m_spawnPoints != null && floorTransition.m_spawnPoints.Length != 0)
            {
                dimensionLevel.AddSpawnPoints(floorTransition.m_spawnPoints);
            }

            tileShape.m_gridPosition = new(gridSize / 2, gridSize / 2);
            tileShape.m_type = floorTransition.m_shapeType;
            var geoNodeVolume = floorTransition.GetComponent<AIG_GeomorphNodeVolume>();
            if (geoNodeVolume == null)
            {
                DebugLog.LogPrefixError(__instance, $"<b><color=red>NULL ERROR</color></b> The Dimension Geomorph for {dimensionIndex} did not have a {typeof(AIG_GeomorphNodeVolume).Name} component on it!");
                __result = count;
                return false;
            }

            floorTransition.m_nodeVolume = geoNodeVolume;
            geoNodeVolume.ConstructVoxelNodeVolume();
            floorTransition.ScanChildrenAreas();
            dimension.m_startTile = LG_Tile.Create(grid, tileShape);
            dimension.m_startTile.m_geoRoot = floorTransition;
            dimension.m_startTile.m_geoRoot.m_tile = dimension.m_startTile;
            floorTransition.transform.SetParent(dimension.DimensionRootTemp.transform);
            floorTransition.m_tile = dimension.m_startTile;
            dimension.m_startTransition = floorTransition;
            dimension.Tiles.Add(dimension.m_startTile);
            __instance.m_dimensions.Add(dimension);
            __instance.m_indexToDimensionMap[dimensionIndex] = dimension;
            __result = count;
            return false;
        }
    }
}