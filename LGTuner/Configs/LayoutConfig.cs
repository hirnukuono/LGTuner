using Expedition;
using GameData;
using GTFO.API;
using LevelGeneration;
using LevelGeneration.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using UnityEngine;

namespace LGTuner.Configs
{
    public sealed class LayoutConfig
    {
        public uint LevelLayoutID { get; set; }
        public Complex[] ExtraComplexResourceToLoad { get; set; } = Array.Empty<Complex>();
        public ZoneOverrideData[] ZoneOverrides { get; set; } = Array.Empty<ZoneOverrideData>();
        public TileOverrideData[] TileOverrides { get; set; } = Array.Empty<TileOverrideData>();

        private TileOverrideData[,] _builtTileOverrides = new TileOverrideData[0, 0];

        [JsonIgnore] public int GridSize { get; private set; }
        [JsonIgnore] public int GridSizeHalf { get; private set; }

        public void Reset(int gridSize)
        {
            Array.ForEach(ZoneOverrides, item => item.Clear());
            Array.Clear(_builtTileOverrides, 0, _builtTileOverrides.Length);

            GridSize = gridSize;
            GridSizeHalf = gridSize / 2;
            _builtTileOverrides = new TileOverrideData[GridSize, GridSize];

            foreach (var tileOverride in TileOverrides)
            {
                if (_builtTileOverrides[tileOverride.X + GridSizeHalf, tileOverride.Z + GridSizeHalf] == null)
                {
                    PutOverrideData(tileOverride);
                }
                else
                {
                    Logger.Error($"Duplicate tile data in layout: {LevelLayoutID}, ({tileOverride.X}, {tileOverride.Z})!");
                }
            }
        }

        public bool TryGetTileData(LG_GridPosition normalGridPosition, out TileOverrideData overrideData)
        {
            var x = normalGridPosition.x + GridSizeHalf;
            if (x >= GridSize)
            {
                Logger.Error($"TryGetTileData grid x was out of range! {x}:{GridSize}");
                overrideData = null;
                return false;
            }

            var z = normalGridPosition.z + GridSizeHalf;
            if (z >= GridSize)
            {
                Logger.Error($"TryGetTileData grid Z was out of range! {z}:{GridSize}");
                overrideData = null;
                return false;
            }
            overrideData = _builtTileOverrides[x, z];
            return overrideData != null;
        }

        public void PutOverrideData(TileOverrideData data)
        {
            _builtTileOverrides[data.X + GridSizeHalf, data.Z + GridSizeHalf] = data;
        }

        public TileOverrideData PopulateTileOverrideForZone(LG_Zone zone, LG_GridPosition normalGridPosition)
        {
            if (!TryGetZoneOverride(zone.LocalIndex, out var zoneOverrideData))
                return null;

            if (TryGetTileData(normalGridPosition, out var tileData))
            {
                tileData.ZoneData = zoneOverrideData;
                TryApplyOverrides(zoneOverrideData, tileData);
                return tileData;
            }
            else
            {
                var newTileData = new TileOverrideData()
                {
                    ZoneData = zoneOverrideData,
                    X = normalGridPosition.x,
                    Z = normalGridPosition.z,
                    OverridePlugWithNoGateChance = zoneOverrideData.OverridePlugWithNoGateChance,
                    PlugWithNoGateChance = zoneOverrideData.PlugWithNoGateChance
                };
                TryApplyOverrides(zoneOverrideData, newTileData);
                PutOverrideData(newTileData);
                return newTileData;
            }
        }
        
        private void TryApplyOverrides(ZoneOverrideData zoneOverrideData, TileOverrideData tileOverrideData)
        {
            if (zoneOverrideData.OverrideGeomorphs && string.IsNullOrEmpty(tileOverrideData.Geomorph))
            {
                var nextGeo = zoneOverrideData.GetNextGeo();
                if (nextGeo.HasValue)
                {
                    tileOverrideData.Geomorph = nextGeo.Value.Geomorph;
                    if (tileOverrideData.Rotation == RotateType.None)
                    {
                        tileOverrideData.Rotation = ZoneOverrideData.DirectionToRotate(nextGeo.Value.Direction);
                    }
                }
            }

            if (zoneOverrideData.OverrideAltitudes && !tileOverrideData.OverrideAltitude)
            {
                var nextAltitude = zoneOverrideData.GetNextAltitude();
                if (nextAltitude.HasValue)
                {
                    tileOverrideData.OverrideAltitude = true;
                    tileOverrideData.Altitude = nextAltitude.Value;
                }
            }
        }

        public bool TryGetZoneOverride(eLocalZoneIndex localIndex, out ZoneOverrideData data)
        {
            data = ZoneOverrides.SingleOrDefault(z => z.LocalIndex == localIndex);
            return data != null;
        }
    }
}
