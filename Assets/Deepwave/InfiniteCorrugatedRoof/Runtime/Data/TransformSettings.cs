using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Deepwave.ICR.Data
{
    [System.Serializable]
    public struct TransformSettings
    {
        public bool enabled;
        [FormerlySerializedAs("horizontalJitter"), Min(0f)] public float horizontalJitterMeters;
        [FormerlySerializedAs("verticalGap"), Min(0f)] public float verticalGapMeters;
        [Min(0f)] public float tiltDegrees;
        [Min(0f)] public float yawDegrees;
        [Min(0f)] public float lengthVarianceMeters;

        public static TransformSettings Default => new()
        {
            enabled = true,
            horizontalJitterMeters = 0f,
            verticalGapMeters = 0f,
            tiltDegrees = 0f,
            yawDegrees = 0f,
            lengthVarianceMeters = 0f
        };
    }
}
