using Deepwave.ICR.Data;
using UnityEngine;

namespace Deepwave.ICR.Utilities
{
    internal static class NoiseUtility
    {
        public static Vector3 ApplyNoise(Vector3 localPosition, NoiseSettings noise, Vector2 panelOffset)
        {
            if (!noise.enabled)
            {
                return localPosition;
            }

            float frequency = 1f / Mathf.Max(0.01f, noise.scale);

            float heightNoise = SampleFractal(localPosition.x, localPosition.y, panelOffset, frequency, noise.detail, 0f);
            float warpNoiseX = SampleFractal(localPosition.x, localPosition.y, panelOffset, frequency, noise.detail, 23.7f);
            float warpNoiseZ = SampleFractal(localPosition.x, localPosition.y, panelOffset, frequency, noise.detail, 57.2f);

            localPosition.x += (warpNoiseX - 0.5f) * 2f * noise.warp;
            localPosition.y += (heightNoise - 0.5f) * 2f * noise.height;
            localPosition.z += (warpNoiseZ - 0.5f) * 2f * noise.warp;

            return localPosition;
        }

        private static float SampleFractal(float x, float z, Vector2 panelOffset, float frequency, float detail, float salt)
        {
            float baseX = (x + panelOffset.x + salt) * frequency;
            float baseZ = (z + panelOffset.y + salt) * frequency;

            float primary = Mathf.PerlinNoise(baseX, baseZ);
            float secondary = Mathf.PerlinNoise(baseX * 2f + 11.3f, baseZ * 2f + 41.7f);

            return Mathf.Lerp(primary, secondary, detail);
        }
    }
}
