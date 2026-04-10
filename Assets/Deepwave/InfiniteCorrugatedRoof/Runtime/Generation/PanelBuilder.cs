using System.Collections.Generic;
using UnityEngine;
using Deepwave.ICR.Data;
using Deepwave.ICR.Utilities;
using Deepwave.ICR.Integration;

namespace Deepwave.ICR.Generation
{
    internal static class PanelBuilder
    {
        internal static void Build(
            List<Vector3> verts,
            List<Vector2> uvs,
            List<Color> colors,
            List<int> tris,
            float xStart,
            float yStart,
            float zStackOffset,
            in BuildContext ctx,
            in PanelSpec spec)
        {
            int colCount = ctx.SectionPoints.Count;
            int segCount = ctx.LengthSegments;
            int baseIndex = verts.Count;

            float yEnd = yStart + spec.Length;
            Vector3 panelOrigin = new(xStart, yStart, 0f);

            // Local cache to avoid struct property access in loops
            var cutSettings = ctx.Settings.Cuts;
            var noiseSettings = ctx.Settings.Noise;
            var texSettings = ctx.TextureSettings;
            var splineMode = ctx.Settings.Spline.mode; // Lấy mode từ settings

            // --- Vertex Generation ---
            for (int y = 0; y <= segCount; y++)
            {
                // Optimization: Use cached inverse segment count
                float tY = y * ctx.InvLengthSegments;
                float yPos = Mathf.Lerp(yStart, yEnd, tY);
                float localY = yPos - yStart;

                for (int c = 0; c < colCount; c++)
                {
                    var point = ctx.SectionPoints[c];

                    // 1. Base Position
                    Vector3 worldPos = panelOrigin;
                    worldPos.x += point.Position.x;
                    worldPos.y += localY;
                    worldPos.z += point.Position.y + zStackOffset;

                    // 2. Apply Cut (Global Space logic)
                    worldPos = CutUtility.ApplyCut(
                        worldPos, cutSettings,
                        ctx.OriginX, ctx.OriginY,
                        ctx.TotalWidth, ctx.TotalHeight,
                        xStart, yStart, spec.Width, spec.Length);

                    // 3. Apply Noise (Local Space logic)
                    Vector3 localPos = worldPos - panelOrigin;
                    localPos = NoiseUtility.ApplyNoise(localPos, noiseSettings, spec.NoiseOffset);

                    // 4. Apply Transform (Jitter/Tilt)
                    Vector3 transformedPos = panelOrigin + (spec.Rotation * localPos) + spec.GapOffset;

                    // 5. Apply Spline (World Deform) -> Using the Cache passed in Ctx
                    Vector3 finalPos = SplineUtility.ApplySpline(
                        transformedPos,
                        in ctx.SplineCache, // Pass by reference
                        splineMode,
                        xStart,
                        spec.Width);

                    verts.Add(finalPos);
                    colors.Add(spec.Color);

                    // 6. UVs
                    uvs.Add(TextureMapper.GetUv(
                        spec.TexIndex,
                        spec.TexOrientation,
                        localPos.y,
                        spec.VertOffset,
                        point,
                        texSettings));
                }
            }

            // --- Triangle Generation ---
            // Standard Grid Triangulation
            int colMinusOne = colCount - 1;
            for (int y = 0; y < segCount; y++)
            {
                int rowCurrent = baseIndex + (y * colCount);
                int rowNext = baseIndex + ((y + 1) * colCount);

                for (int c = 0; c < colMinusOne; c++)
                {
                    int bl = rowCurrent + c;
                    int br = bl + 1;
                    int tl = rowNext + c;
                    int tr = tl + 1;

                    tris.Add(bl);
                    tris.Add(tl);
                    tris.Add(br);

                    tris.Add(br);
                    tris.Add(tl);
                    tris.Add(tr);
                }
            }
        }
    }
}
