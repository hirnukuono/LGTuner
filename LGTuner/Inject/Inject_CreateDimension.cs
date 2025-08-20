using AIGraph;
using Expedition;
using GameData;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using LevelGeneration.Core;
using LGTuner.Configs;
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

        private static bool Dim250(LG_Floor __instance, uint seed, bool skipMainDimension = true)
        {
            int gridSize = 40;
            if (Builder.LevelGenExpedition.DimensionDatas == null || Builder.LevelGenExpedition.DimensionDatas.Count <= 0)
            {
                Dimension.CalculateAllDimensionBounds();
                return false;
            }
            XXHashSequence xXHashSequence = new XXHashSequence(seed);
            Vector3 up = Vector3.up;
            int num = 0;
            float num2 = 150f;
            for (int i = 0; i < Builder.LevelGenExpedition.DimensionDatas.Count; i++)
            {
                DimensionInExpeditionData dimensionInExpeditionData = Builder.LevelGenExpedition.DimensionDatas[i];
                if (dimensionInExpeditionData.DimensionIndex != eDimensionIndex.ARENA_DIMENSION && dimensionInExpeditionData.TryGetDimensionData(out var dimensionData))
                {
                    DimensionData dimensionData2 = dimensionData.DimensionData;
                    if (dimensionInExpeditionData.Enabled && (!skipMainDimension || dimensionInExpeditionData.DimensionIndex != eDimensionIndex.Reality) && !__instance.m_createdDimensionSet.Contains(dimensionInExpeditionData.DimensionIndex))
                    {
                        num2 += dimensionData2.VerticalExtentsDown;
                        Vector3 position = up * num2;
                        __instance.CreateDimension(xXHashSequence.NextSubSeed(), dimensionInExpeditionData.DimensionIndex, false, dimensionData2, position, gridSize, 64);
                        __instance.m_createdDimensionSet.Add(dimensionInExpeditionData.DimensionIndex);
                        Logger.Info($"created dim {dimensionInExpeditionData.DimensionIndex} pos {position} ..");

                        num2 += dimensionData2.VerticalExtentsUp; // removed +50, eeeediot hirnu
                        num++;
                    }
                }
            }
            float num3 = 0f;
            float num4 = 0f;
            for (int j = 0; j < Builder.LevelGenExpedition.DimensionDatas.Count; j++)
            {
                DimensionInExpeditionData dimensionInExpeditionData2 = Builder.LevelGenExpedition.DimensionDatas[j];
                if (dimensionInExpeditionData2.DimensionIndex == eDimensionIndex.ARENA_DIMENSION && dimensionInExpeditionData2.TryGetDimensionData(out var dimensionData3))
                {
                    gridSize = 10;
                    if (dimensionData3.DimensionData.VerticalExtentsDown > num3)
                    {
                        num3 = dimensionData3.DimensionData.VerticalExtentsDown;
                    }
                    if (dimensionData3.DimensionData.VerticalExtentsUp > num3)
                    {
                        num4 = num3;
                    }
                }
            }
            int num5 = 0;
            Vector3 right = Vector3.right;
            for (int k = 0; k < SNet.Slots.PlayerSlots.Length; k++)
            {
                num2 += num3;
                float num6 = 0f;
                for (int l = 0; l < Builder.LevelGenExpedition.DimensionDatas.Count; l++)
                {
                    DimensionInExpeditionData dimensionInExpeditionData3 = Builder.LevelGenExpedition.DimensionDatas[l];
                    if (dimensionInExpeditionData3.DimensionIndex != eDimensionIndex.ARENA_DIMENSION)
                    {
                        continue;
                    }
                    if (!dimensionInExpeditionData3.TryGetDimensionData(out var dimensionData4))
                    {
                        Debug.LogError("Could not find datablock for arena: datablockID: " + dimensionInExpeditionData3.DimensionData);
                        continue;
                    }
                    eDimensionIndex fDimensionIndex = (eDimensionIndex)(21 - (num5 + 1));
                    if (__instance.m_createdDimensionSet.Contains(fDimensionIndex))
                    {
                        DebugLog.LogPrefixError(__instance, $"Enemy dimension ({fDimensionIndex}) had already been created! If there are dimension enemies in the level, dimensionIndex {eDimensionIndex.Dimension_17}-{eDimensionIndex.Dimension_20} needs to be free (one for each player slot).");
                        continue;
                    }
                    DimensionData dimensionData5 = dimensionData4.DimensionData;
                    if (dimensionInExpeditionData3.Enabled)
                    {
                        GameObject customGeomorph = GameDataBlockBase<ComplexResourceSetDataBlock>.GetBlock(dimensionData5.DimensionResourceSetID).GetCustomGeomorph(dimensionData5.DimensionGeomorph);
                        if (customGeomorph == null)
                        {
                            DebugLog.LogPrefixError(__instance, $"The dimension geomorph for ({dimensionData4.persistentID}) {dimensionData4.name} could not be found!");
                            continue;
                        }
                        float num7 = 32f;
                        LG_Dimension component = customGeomorph.GetComponent<LG_Dimension>();
                        if (component != null && component.TryGetDimensionBounds(out var bounds))
                        {
                            num7 += bounds.extents.x;
                        }
                        num6 += num7;
                        Vector3 position2 = up * num2 + right * num6;

                        __instance.CreateDimension(xXHashSequence.NextSubSeed(), fDimensionIndex, true, dimensionData5, position2, 10, 64);
                        __instance.m_createdDimensionSet.Add(fDimensionIndex);
                        DimensionSlotIndex gaa = new() { DimensionDataBlockID = 14, PlayerSlotIndex = (uint)k };
                        __instance.m_dimensionArenaMap.Add(gaa, __instance.m_dimensions[__instance.m_dimensions.Count - 1]);
                        Logger.Info($"created dim {fDimensionIndex} pos {position2} ..");

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
        private static bool Prefix(LG_Floor __instance, uint seed, eDimensionIndex dimensionIndex, bool arenaDimension, ref DimensionData dimensionData, Vector3 position, ref int __result, int gridSize, float cellSize = 64f)
        {
            if (arenaDimension) return true;
            gridSize = 40;
            SubComplex subcomplex = Builder.LayerBuildDatas[0].m_zoneBuildDatas[0].SubComplex;
            if (dimensionData.IsStaticDimension) return true;
            LayoutConfig dimLayer = null;
            var layout = LevelLayoutDataBlock.GetBlock(dimensionData.LevelLayoutData);
            ConfigManager.TryGetConfig(layout.persistentID, out dimLayer);
            if (dimLayer == null) return true;
            if (dimLayer.TileOverrides.Length == 0) return true;
            Logger.Info($"in createdim prefix gridsize {gridSize} ..");
            cellSize = 64f;
            GameObject customGeomorph = null;
            XXHashSequence xXHashSequence = new XXHashSequence(seed);
            Vector3 gridPosition = position - new Vector3(cellSize, 0f, cellSize) * gridSize * 0.5f;
            LG_TileShape lG_TileShape = __instance.CreateTileShape(LG_TileShapeType.s1x1, cellSize, ref gridPosition);
            LG_Grid lG_Grid = CellGridBase<LG_Grid, LG_Tile, LG_Cell>.Create(cellSize, gridPosition);
            lG_Grid.SetupCells(gridSize, gridSize);
            lG_Grid.InititalizeCells();
            int count = __instance.m_dimensions.Count;
            Dimension dimension = new Dimension(lG_Grid, dimensionIndex, count, dimensionData, position, arenaDimension);

            DebugLog.LogPrefix(__instance, $"Created Dimension (Index: {dimensionIndex}, gridSize: {gridSize}, cellSize: {cellSize}).");
            customGeomorph = dimension.ResourceData.GetCustomGeomorph(dimensionData.DimensionGeomorph);
            if (layout != null)
            {
                if (dimLayer.TileOverrides[0].X == 0 && dimLayer.TileOverrides[0].Z == 0 && !string.IsNullOrEmpty(dimLayer.TileOverrides[0].Geomorph))
                {
                    customGeomorph = AssetAPI.GetLoadedAsset(dimLayer.TileOverrides[0].Geomorph)?.Cast<GameObject>();
                    Logger.Info($" - dim {dimensionIndex} elevator overriden! {customGeomorph.name}");
                }
            }

            if (customGeomorph == null)
            {
                DebugLog.LogPrefixError(__instance, $"<b><color=red>NULL ERROR</color></b> Could not get the Dimension Geomorph prefab for dimension {dimensionIndex}, are you using a resource set in the DimensionDataBlock that contains the Dimension Geomorph prefab under the \"Custom Geomorphs_Objective_1x1\" header?");
                __result = count;
                return false;
            }
            LG_FloorTransition lG_FloorTransition = __instance.CreateFloorTransition(xXHashSequence.NextSubSeed(), dimension, position, Quaternion.identity, customGeomorph, dimension.DimensionData.IsStaticDimension);
            if (lG_FloorTransition == null)
            {
                DebugLog.LogPrefixError(__instance, $"<b><color=red>NULL ERROR</color></b> The LG_FloorTransition of dimension {dimensionIndex} was null! Does the dimension prefab have a LG_FloorTransition component on its root?");
                __result = count;
                return false;
            }
            LG_Dimension lG_Dimension = lG_FloorTransition.GetComponent<LG_Dimension>();
            if (lG_Dimension == null)
            {
                lG_Dimension = lG_FloorTransition.gameObject.AddComponent<LG_Dimension>();
                DebugLog.LogPrefixError(__instance, $"<b><color=red>NULL ERROR</color></b> The dimension {dimensionIndex} had no {typeof(LG_Dimension).Name} component, so added one automatically!");
            }
            dimension.DimensionLevel = lG_Dimension;
            if (lG_FloorTransition.m_spawnPoints != null && lG_FloorTransition.m_spawnPoints.Length != 0)
            {
                lG_Dimension.AddSpawnPoints(lG_FloorTransition.m_spawnPoints);
            }
            lG_TileShape.m_gridPosition = new(gridSize / 2, gridSize / 2);
            lG_TileShape.m_type = lG_FloorTransition.m_shapeType;
            AIG_GeomorphNodeVolume component = lG_FloorTransition.GetComponent<AIG_GeomorphNodeVolume>();
            if (component == null)
            {
                DebugLog.LogPrefixError(__instance, $"<b><color=red>NULL ERROR</color></b> The Dimension Geomorph for {dimensionIndex} did not have a {typeof(AIG_GeomorphNodeVolume).Name} component on it!");
                __result = count;
                return false;
            }
            lG_FloorTransition.m_nodeVolume = component;
            component.ConstructVoxelNodeVolume();
            lG_FloorTransition.ScanChildrenAreas();
            dimension.m_startTile = LG_Tile.Create(lG_Grid, lG_TileShape);
            dimension.m_startTile.m_geoRoot = lG_FloorTransition;
            dimension.m_startTile.m_geoRoot.m_tile = dimension.m_startTile;
            lG_FloorTransition.transform.SetParent(dimension.DimensionRootTemp.transform);
            lG_FloorTransition.m_tile = dimension.m_startTile;
            dimension.m_startTransition = lG_FloorTransition;
            dimension.Tiles.Add(dimension.m_startTile);
            __instance.m_dimensions.Add(dimension);
            __instance.m_indexToDimensionMap[dimensionIndex] = dimension;
            __result = count;
            return false;
        }
    }
}