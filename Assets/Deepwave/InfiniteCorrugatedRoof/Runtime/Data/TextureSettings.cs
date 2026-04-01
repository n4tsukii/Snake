using Deepwave.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deepwave.ICR.Data
{
    [System.Serializable]
    public struct TextureSettings
    {
        public bool enabled;
        public List<Color> panelColors;
        [Min(0.1f)] public float verticalTiling;
        [DynamicRange(0f, 1f)] public DynamicFloat verticalOffset;
        [DynamicRange(0, 7)] public DynamicInt wearLevel;
        [DynamicRange(0, "panelColors")] public DynamicInt panelColorIndex;

        public static TextureSettings Default => new()
        {
            enabled = true,
            panelColors = new List<Color> { Color.white },
            verticalTiling = 1f,
            verticalOffset = DynamicFloat.Default(0f),
            wearLevel = DynamicInt.Default(0),
            panelColorIndex = DynamicInt.Default(0)
        };
    }
}
