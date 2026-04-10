using Deepwave.ICR.Data;
using Deepwave.ICR.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Deepwave.ICR.Generation
{
    internal static class WaveBuilder
    {
        private const float Epsilon = 1e-5f;

        public static List<SectionPoint> Build(
            float width,
            int waveCount,
            float waveAmplitude,
            bool isLowestLOD,
            List<float> keys,
            AnimationCurve profile)
        {
            int capacity = waveCount * keys.Count;
            var cols = new List<SectionPoint>(capacity);

            // Pre-calculate constants
            float invWaves = 1f / waveCount;
            float segmentWidth = width * invWaves; // Chiều rộng thực tế của 1 sóng

            // Check seamless
            bool seamless = Mathf.Abs(profile.Evaluate(0f) - profile.Evaluate(1f)) < Epsilon;

            for (int waveIndex = 0; waveIndex < waveCount; waveIndex++)
            {
                bool skipT0 = seamless && waveIndex > 0;
                int keyCount = keys.Count;

                for (int i = 0; i < keyCount; i++)
                {
                    if (skipT0 && i == 0) continue;

                    float t = keys[i]; // Normalized Time (0..1) trong 1 chu kỳ sóng

                    // 1. Tính Position
                    float y = profile.Evaluate(t) * waveAmplitude;
                    float x = (waveIndex + t) * segmentWidth;

                    // 3. Tính UV & Filter Virtual
                    var localU = TextureMapper.CalculateLocalU(waveIndex, t);

                    bool isInnerUv = localU > Epsilon && localU < (1f - Epsilon);
                    bool isInnerPos = x > Epsilon && x < (width - Epsilon);

                    if (isLowestLOD && isInnerUv && isInnerPos) continue;

                    // Add point với Normal đã tính
                    cols.Add(new SectionPoint
                    (
                        new Vector2(x, y),
                        localU
                    ));
                }
            }
            return cols;
        }
    }
}