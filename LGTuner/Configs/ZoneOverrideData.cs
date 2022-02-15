using GameData;
using System;
using System.Collections.Generic;
using System.Text;

namespace LGTuner.Configs
{
    public sealed class ZoneOverrideData
    {
        public eLocalZoneIndex LocalIndex { get; set; } = eLocalZoneIndex.Zone_0;
        public bool OverrideGeomorphs { get; set; } = false;
        public GeomorphData[] Geomorphs { get; set; } = Array.Empty<GeomorphData>();
        public bool OverrideAltitudes { get; set; } = false;
        public int[] Altitudes { get; set; } = Array.Empty<int>();
        public bool OverridePlugs { get; set; } = false;
        public string[] Plugs { get; set; } = Array.Empty<string>();
        public bool OverridePlugWithNoGateChance { get; set; } = false;
        public float PlugWithNoGateChance { get; set; } = 0.5f;

        private int _curGeomorphIndex = 0;
        private int _curAltitudeIndex = 0;
        private int _curPlugIndex = 0;

        public void Clear()
        {
            _curGeomorphIndex = 0;
            _curAltitudeIndex = 0;
            _curPlugIndex = 0;
        }

        public GeomorphData? GetNextGeo()
        {
            if (Geomorphs == null || Geomorphs.Length == 0)
                return null;

            return Geomorphs[_curGeomorphIndex++ % Geomorphs.Length];
        }

        public int? GetNextAltitude()
        {
            if (Altitudes == null || Altitudes.Length == 0)
                return null;

            return Altitudes[_curAltitudeIndex++ % Altitudes.Length];
        }

        public string GetNextPlug()
        {
            if (Plugs == null || Plugs.Length == 0)
                return string.Empty;

            return Plugs[_curPlugIndex++ % Plugs.Length];
        }

        public enum Direction
        {
            Unchanged,
            Random,
            Forward,
            Backward,
            Left,
            Right
        }

        public static RotateType DirectionToRotate(Direction direction)
        {
            return direction switch
            {
                Direction.Unchanged => RotateType.None,
                Direction.Random => RotateType.Towards_Random,
                Direction.Forward => RotateType.Towards_Forward,
                Direction.Backward => RotateType.Towards_Backward,
                Direction.Left => RotateType.Towards_Left,
                Direction.Right => RotateType.Towards_Right,
                _ => throw new NotImplementedException($"DirectionType: {direction} is not supported"),
            };
        }

        public struct GeomorphData
        {
            public string Geomorph { get; set; }
            public Direction Direction { get; set; }
        }
    }
}
