using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Deepwave.ICR.Data;
using Deepwave.ICR.Utilities;
using Deepwave.ICR.Integration;

namespace Deepwave.ICR.Generation
{
    internal static class MeshBuilder
    {
        public static void Build(
            Mesh mesh,
            BuildSettings settings,
            AnimationCurve waveProfile,
            in LodProfile lodProfile,
            in SplineUtility.SplineCache splineCache)
        {
            // 1. Prepare Data
            var keyTimes = CurveUtility.GetKeyTimesUsed(waveProfile, lodProfile.KeyframeStep);
            int effectiveWaveCount = settings.WaveCountPerPanel;

            // 2. Pre-calculate Waves (Delegate to WaveCalculator)
            var columns = WaveBuilder.Build(
                settings.PanelWidth,
                effectiveWaveCount,
                settings.WaveAmplitude,
                lodProfile.IsLowestLOD,
                keyTimes,
                waveProfile);

            // 3. Layout Planning (Delegate to LayoutPlanner)
            var panelSpecs = LayoutPlanner.GenerateSpecs(settings, settings.TextureMapping);

            // 4. Allocation Estimation
            float waveOverlapWidth = settings.PanelWidth / Mathf.Max(1, settings.WaveCountPerPanel);

            // Tính toán bounds tổng thể cho UV mapping
            var totalHeight = settings.PanelLength;
            if (settings.Rows > 1)
                totalHeight = (settings.Rows * settings.PanelLength) - ((settings.Rows - 1) * settings.VerticalOverlap);

            float totalWidth = 0f;
            if (settings.Columns > 0)
                totalWidth = (settings.Columns * settings.PanelWidth) - ((settings.Columns - 1) * waveOverlapWidth);

            int totalPanelCount = LayoutPlanner.GetTotalPanelCount(panelSpecs);
            int vertsPerPanel = columns.Count * (lodProfile.LengthSegmentsPerPanel + 1);
            int totalVerts = totalPanelCount * vertsPerPanel;
            int totalTris = totalPanelCount * (columns.Count - 1) * lodProfile.LengthSegmentsPerPanel * 6;

            bool hasThickness = settings.Thickness > 0f;
            if (hasThickness)
            {
                totalVerts *= 2;
                totalTris *= 2;
            }

            // 5. Initialize Buffers
            var vertices = new List<Vector3>(totalVerts);
            var uvs = new List<Vector2>(totalVerts);
            var colors = new List<Color>(totalVerts);
            var triangles = new List<int>(totalTris);

            // 6. Build Context
            var ctx = new BuildContext(
                settings,
                settings.TextureMapping,
                columns,
                splineCache,
                totalWidth,
                totalHeight,
                lodProfile.LengthSegmentsPerPanel
            );

            // 7. Main Construction Loop
            float currentX = 0f;
            float layerHeightStep = settings.Thickness + settings.OverlapDistance;

            for (int x = 0; x < settings.Columns; x++)
            {
                float currentY = 0f;
                var columnPanels = panelSpecs[x];
                float overlapX = (x == 0) ? 0f : waveOverlapWidth;

                currentX += settings.PanelWidth - overlapX;
                float panelStartX = currentX - (settings.PanelWidth - overlapX);
                float baseZHeight = x * layerHeightStep;

                int panelCount = columnPanels.Count;
                for (int z = 0; z < panelCount; z++)
                {
                    var spec = columnPanels[z];
                    float stackOffsetZ = baseZHeight + (z * layerHeightStep);

                    PanelBuilder.Build(vertices, uvs, colors, triangles, panelStartX, currentY, stackOffsetZ, in ctx, in spec);

                    currentY += spec.Length - settings.VerticalOverlap;
                }
            }

            // 8. Post-Processing (Delegate to GeometryModifier)
            if (hasThickness)
            {
                GeometryModifier.AddThickness(vertices, uvs, colors, triangles, settings.Thickness);
            }

            // 9. Apply to Unity Mesh
            ApplyDataToMesh(mesh, vertices, uvs, colors, triangles);
        }

        private static void ApplyDataToMesh(Mesh mesh, List<Vector3> verts, List<Vector2> uvs, List<Color> colors, List<int> tris)
        {
            mesh.Clear();
            // Tự động chuyển format index nếu mesh quá lớn (>65k verts)
            if (verts.Count > 65535) mesh.indexFormat = IndexFormat.UInt32;

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(tris, 0);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}