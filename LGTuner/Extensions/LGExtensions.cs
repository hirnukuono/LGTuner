using LevelGeneration;
using LevelGeneration.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LGTuner
{
    public static class LGExtensions
    {
        public static bool IsSame(this LG_GridPosition position, LG_GridPosition other)
        {
            return (position.x == other.x) && (position.z == other.z);
        }

        public static LG_GridPosition GetNormal(this LG_GridPosition position, int gridSize = -1)
        {
            if (gridSize < 0)
            {
                return new LG_GridPosition()
                {
                    x = position.x + BuilderInfo.GridZeroOffset,
                    z = position.z + BuilderInfo.GridZeroOffset
                };
            }
            else
            {
                var gridSizeHalf = gridSize / 2;

                return new LG_GridPosition()
                {
                    x = position.x - gridSizeHalf,
                    z = position.z - gridSizeHalf
                };
            }
        }

        public static Vector3 GetPositionNormal(this LG_Grid grid, int x, int z, int gridSize = -1)
        {
            if (gridSize < 0)
            {
                return grid.GetPosition(x - BuilderInfo.GridZeroOffset, z - BuilderInfo.GridZeroOffset);
            }
            else
            {
                var gridSizeHalf = gridSize / 2;

                return grid.GetPosition(x + gridSizeHalf, z + gridSizeHalf);
            }
        }

        public static int GetGridSize(this Dimension dimension)
        {
            return dimension.Grid.m_sizeX;
        }

        public static bool TryGetDimension(this LG_Floor floor, int dimensionIndex, out Dimension dimension)
        {
            if (Enum.IsDefined(typeof(eDimensionIndex), dimensionIndex))
            {
                return floor.GetDimension((eDimensionIndex)dimensionIndex, out dimension);
            }
            dimension = null;
            return false;
        }

        public static bool TryGetDimension(this LG_Floor floor, eDimensionIndex dimensionIndex, out Dimension dimension)
        {
            if (Enum.IsDefined(typeof(eDimensionIndex), dimensionIndex))
            {
                return floor.GetDimension(dimensionIndex, out dimension);
            }
            dimension = null;
            return false;
        }
    }
}
