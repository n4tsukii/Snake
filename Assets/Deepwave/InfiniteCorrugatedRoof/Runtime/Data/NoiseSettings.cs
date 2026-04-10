using System;
using UnityEngine;

namespace Deepwave.ICR.Data
{
    [Serializable]
    public struct NoiseSettings
    {
        public bool enabled;
        [Min(0f)] public float height;
        [Min(0f)] public float warp;
        [Min(0.01f)] public float scale;
        [Range(0f, 1f)] public float detail;

        public static NoiseSettings Default => new()
        {
            enabled = false,
            height = 0.01f,
            warp = 0.002f,
            scale = 0.35f,
            detail = 0.35f
        };
    }
}
