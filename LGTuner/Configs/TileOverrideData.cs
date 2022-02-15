using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace LGTuner.Configs
{
    public sealed class TileOverrideData
    {
        public int X { get; set; }
        public int Z { get; set; }
        public RotateType Rotation { get; set; } = RotateType.None;
        public string Geomorph { get; set; } = string.Empty;
        public bool OverrideAltitude { get; set; } = false;
        public int Altitude { get; set; } = 0;
        public string ForwardPlug { get; set; } = string.Empty;
        public string BackwardPlug { get; set; } = string.Empty;
        public string LeftPlug { get; set; } = string.Empty;
        public string RightPlug { get; set; } = string.Empty;
        public bool OverridePlugWithNoGateChance { get; set; } = false;
        public float PlugWithNoGateChance { get; set; } = 0.5f;
        [JsonIgnore] public ZoneOverrideData ZoneData { get; set; }
    }
}
