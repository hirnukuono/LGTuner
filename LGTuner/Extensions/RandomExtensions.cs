using UnityEngine;

namespace LGTuner.Extensions
{
    public static class RandomExtensions
    {
        public static uint NextUInt(this System.Random random)
        {
            uint thirtyBits = (uint)random.Next(1 << 30);
            uint twoBits = (uint)random.Next(1 << 2);
            return (thirtyBits << 2) | twoBits;
        }

        public static float NextFloat01(this System.Random random)
        {
            return Mathf.Clamp01(random.NextSingle());
        }

        public static int ToSeed(this uint seed)
        {
            return unchecked((int)seed);
        }
    }
}
