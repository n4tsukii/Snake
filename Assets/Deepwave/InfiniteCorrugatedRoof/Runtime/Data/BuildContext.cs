using System.Collections.Generic;
using Deepwave.ICR.Integration;

namespace Deepwave.ICR.Data
{
    internal readonly struct BuildContext
    {
        public readonly BuildSettings Settings;
        public readonly TextureSettings TextureSettings;
        public readonly List<SectionPoint> SectionPoints;
        public readonly SplineUtility.SplineCache SplineCache; // Cache passed from outside

        // Pre-calculated global bounds for Cuts
        public readonly float OriginX;
        public readonly float OriginY;
        public readonly float TotalWidth;
        public readonly float TotalHeight;

        public readonly int LengthSegments;
        public readonly float InvLengthSegments; // Optimization: Cache 1/segments

        public BuildContext(
            BuildSettings settings,
            TextureSettings textureSettings,
            List<SectionPoint> columns,
            in SplineUtility.SplineCache splineCache,
            float totalWidth,
            float totalHeight,
            int segments)
        {
            Settings = settings;
            TextureSettings = textureSettings;
            SectionPoints = columns;
            SplineCache = splineCache;

            OriginX = 0f; // Could be passed in if needed
            OriginY = 0f;
            TotalWidth = totalWidth;
            TotalHeight = totalHeight;

            LengthSegments = segments;
            InvLengthSegments = segments > 0 ? 1f / segments : 0f;
        }
    }
}
