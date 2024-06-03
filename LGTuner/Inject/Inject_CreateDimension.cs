using AIGraph;
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
    [HarmonyPatch(typeof(LG_Floor), nameof(LG_Floor.CreateDimension))]
    internal static class Inject_CreateDimension
    {
        //private static LayoutConfig _configContext = null;

        [HarmonyPrefix]
        private static bool Prefix(LG_Floor __instance, uint seed, eDimensionIndex dimensionIndex, bool arenaDimension, DimensionData dimensionData, Vector3 position, ref int __result, int gridSize = 10, float cellSize = 64f)
        {
            if (dimensionData.IsStaticDimension) return true;
            LayoutConfig dimLayer = null;
            var layout = LevelLayoutDataBlock.GetBlock(dimensionData.LevelLayoutData);
            ConfigManager.TryGetConfig(layout.persistentID, out dimLayer);
            if (dimLayer == null) return true;
            if (dimLayer.TileOverrides.Length == 0) return true;
            gridSize = 10;
            cellSize = 64f;
            LG_Zone zone = null;
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