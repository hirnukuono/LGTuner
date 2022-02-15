using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace LGTuner
{
    public static class RandomExtensions
    {
        public static uint NextUint(this Random random)
        {
            uint thirtyBits = (uint)random.Next(1 << 30);
            uint twoBits = (uint)random.Next(1 << 2);
            return (thirtyBits << 2) | twoBits;
        }

        public static float NextFloat(this Random random)
        {
            return Mathf.Clamp01((float)random.NextDouble());
        }

        public static int ToSeed(this uint seed)
        {
            return unchecked((int)seed);
        }
    }
}
