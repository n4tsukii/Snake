using System;
using UnityEngine.Serialization;

namespace Deepwave.ICR.Data
{
    [Serializable]
    public enum CutMode { Horizontal, Diagonal }

    [Serializable]
    public struct CutSettings
    {
        public bool enabled;
        public CutMode mode;
        [FormerlySerializedAs("startAngle")] public float startAngleDegrees;
        [FormerlySerializedAs("endAngle")] public float endAngleDegrees;

        public static CutSettings Default => new() { enabled = true };
    }
}
