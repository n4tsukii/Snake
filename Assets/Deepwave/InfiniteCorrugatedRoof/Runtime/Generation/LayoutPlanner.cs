using Deepwave.ICR.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Deepwave.ICR.Generation
{
    internal static class LayoutPlanner
    {
        // Context để gom nhóm các biến chạy, tránh truyền quá nhiều tham số lẻ tẻ
        private struct GenContext
        {
            public int Seed;
            public int ColumnIdx;
            public int PanelIdx;
            public int Stream; // Biến đếm để hash không bị trùng lặp
        }

        public static List<PanelSpec>[] GenerateSpecs(BuildSettings settings, TextureSettings texSettings)
        {
            var specs = new List<PanelSpec>[settings.Columns];

            // Pre-calc các thông số tĩnh để tránh tính lại trong vòng lặp
            var panelColors = texSettings.panelColors;
            int wearMin = Mathf.RoundToInt(texSettings.wearLevel.range.min);
            int wearMax = Mathf.RoundToInt(texSettings.wearLevel.range.max);
            if (wearMin > wearMax) (wearMin, wearMax) = (wearMax, wearMin);

            // Tính toán trước variance dựa trên settings.PanelTransform.enabled
            float effectiveVariance = settings.PanelTransform.enabled ? settings.PanelTransform.lengthVarianceMeters : 0f;

            for (int x = 0; x < settings.Columns; x++)
            {
                specs[x] = GenerateColumn(settings, texSettings, x, effectiveVariance, wearMin, wearMax);
            }

            return specs;
        }

        public static int GetTotalPanelCount(List<PanelSpec>[] specs)
        {
            int count = 0;
            for (int i = 0; i < specs.Length; i++) count += specs[i].Count;
            return count;
        }

        // Xử lý logic lấp đầy cho một cột duy nhất
        private static List<PanelSpec> GenerateColumn(
            BuildSettings settings,
            TextureSettings texSettings,
            int colIndex,
            float lengthVariance,
            int wearMin, int wearMax)
        {
            var columnSpecs = new List<PanelSpec>(settings.Rows);
            float currentLen = 0f;
            float targetLength = settings.PanelLength * settings.Rows;
            const float MinLen = 0.1f;

            // Hàm helper để tạo và thêm panel vào danh sách
            void AddPanel(float length)
            {
                var ctx = new GenContext
                {
                    Seed = settings.RandomSeed,
                    ColumnIdx = colIndex,
                    PanelIdx = columnSpecs.Count,
                    Stream = 0
                };

                var spec = CreatePanelSpec(settings, texSettings, length, ctx, wearMin, wearMax);
                columnSpecs.Add(spec);
            }

            // Phase 1: Xây dựng các hàng tiêu chuẩn
            for (int z = 0; z < settings.Rows; z++)
            {
                int panelIdx = columnSpecs.Count;

                // Tính độ dài ngẫu nhiên, chịu ảnh hưởng bởi settings.PanelTransform
                float variance = HashRange(settings.RandomSeed, colIndex, panelIdx, 0, 0f, lengthVariance);
                float len = Mathf.Max(MinLen, settings.PanelLength - variance);
                float remaining = targetLength - currentLen;

                // Gộp vào panel trước nếu phần còn lại quá ngắn (tránh mảnh vụn)
                if (remaining < MinLen && columnSpecs.Count > 0)
                {
                    ExtendLastSpec(columnSpecs, remaining);
                    currentLen += remaining;
                    return columnSpecs; // Đã lấp đầy cột
                }

                if (len > remaining) len = remaining;

                AddPanel(len);
                currentLen += len;
            }

            // Phase 2: Lấp đầy phần còn thiếu (Tail logic)
            while (targetLength - currentLen > MinLen)
            {
                float tailRemaining = targetLength - currentLen;
                // Tính toán overlap nếu không phải panel đầu tiên
                float requiredLen = tailRemaining + (columnSpecs.Count > 0 ? settings.VerticalOverlap : 0f);
                float extraLen = Mathf.Min(settings.PanelLength, requiredLen);

                if (extraLen < MinLen && columnSpecs.Count > 0)
                {
                    ExtendLastSpec(columnSpecs, tailRemaining);
                    break;
                }

                AddPanel(extraLen);

                // Trừ đi phần overlap khi tính tiến độ chiều dài thực tế
                float contribution = extraLen - (columnSpecs.Count > 1 ? settings.VerticalOverlap : 0f);
                currentLen += contribution;
            }

            return columnSpecs;
        }

        private static PanelSpec CreatePanelSpec(
            BuildSettings settings,
            TextureSettings texSettings,
            float length,
            GenContext ctx,
            int wearMin, int wearMax)
        {
            // Các giá trị mặc định
            int wearIndex = texSettings.wearLevel.value;
            float vOffset = texSettings.verticalOffset.value;
            int colorIdx = 0; // Mặc định hoặc lấy từ settings nếu không random

            // Xử lý Texture Randomization
            if (settings.TextureMapping.enabled)
            {
                if (texSettings.panelColorIndex.randomize)
                {
                    float val = HashRange(ctx.Seed, ctx.ColumnIdx, ctx.PanelIdx, ctx.Stream++, texSettings.panelColorIndex.range.min, texSettings.panelColorIndex.range.max);
                    colorIdx = Mathf.Clamp(Mathf.RoundToInt(val), 0, texSettings.panelColors.Count - 1);
                }
                else
                {
                    colorIdx = Mathf.Clamp(Mathf.RoundToInt(texSettings.panelColorIndex.value), 0, texSettings.panelColors.Count - 1);
                }

                if (texSettings.verticalOffset.randomize)
                    vOffset = HashRange(ctx.Seed, ctx.ColumnIdx, ctx.PanelIdx, ctx.Stream++, texSettings.verticalOffset.range.min, texSettings.verticalOffset.range.max);

                if (texSettings.wearLevel.randomize)
                    wearIndex = HashRangeInt(ctx.Seed, ctx.ColumnIdx, ctx.PanelIdx, ctx.Stream++, wearMin, wearMax);
            }

            // Xử lý Transform (Jitter/Rotation) - Chỉ tính khi enabled
            var pt = settings.PanelTransform;
            bool tfEnabled = pt.enabled;

            float jitter = tfEnabled ? pt.horizontalJitterMeters : 0f;
            float tilt = tfEnabled ? pt.tiltDegrees : 0f;
            float yaw = tfEnabled ? pt.yawDegrees : 0f;
            float gap = tfEnabled ? pt.verticalGapMeters : 0f;

            return new PanelSpec
            {
                Width = settings.PanelWidth,
                Length = length,
                TexIndex = wearIndex,
                VertOffset = vOffset,
                // Stream tiếp tục tăng để đảm bảo các chỉ số noise không trùng nhau
                TexOrientation = HashRangeInt(ctx.Seed, ctx.ColumnIdx, ctx.PanelIdx, ctx.Stream++, 0, 3),
                Color = texSettings.panelColors[colorIdx],

                GapOffset = new Vector3(
                    HashRangeSym(ctx.Seed, ctx.ColumnIdx, ctx.PanelIdx, ctx.Stream++, jitter),
                    HashRange(ctx.Seed, ctx.ColumnIdx, ctx.PanelIdx, ctx.Stream++, 0f, gap),
                    HashRangeSym(ctx.Seed, ctx.ColumnIdx, ctx.PanelIdx, ctx.Stream++, jitter)
                ),

                Rotation = Quaternion.Euler(
                    HashRangeSym(ctx.Seed, ctx.ColumnIdx, ctx.PanelIdx, ctx.Stream++, tilt),
                    HashRangeSym(ctx.Seed, ctx.ColumnIdx, ctx.PanelIdx, ctx.Stream++, yaw),
                    HashRangeSym(ctx.Seed, ctx.ColumnIdx, ctx.PanelIdx, ctx.Stream++, tilt)
                ),

                NoiseOffset = new Vector2(
                    HashRange(ctx.Seed, ctx.ColumnIdx, ctx.PanelIdx, ctx.Stream++, -1000f, 1000f),
                    HashRange(ctx.Seed, ctx.ColumnIdx, ctx.PanelIdx, ctx.Stream++, -1000f, 1000f)
                )
            };
        }

        private static void ExtendLastSpec(List<PanelSpec> columnSpecs, float addedLength)
        {
            int idx = columnSpecs.Count - 1;
            var spec = columnSpecs[idx];
            spec.Length += addedLength;
            columnSpecs[idx] = spec; // Cập nhật struct trong list
        }

        #region Hashing Helpers
        // Giữ nguyên logic Hash nhưng đưa vào region cho gọn code chính
        private static float HashRange(int seed, int c, int p, int s, float min, float max)
        {
            return Mathf.Lerp(min, max, Hash01(seed, c, p, s));
        }

        private static float HashRangeSym(int seed, int c, int p, int s, float max)
        {
            if (max <= 0f) return 0f;
            return Mathf.Lerp(-max, max, Hash01(seed, c, p, s));
        }

        private static int HashRangeInt(int seed, int c, int p, int s, int min, int max)
        {
            if (max <= min) return min;
            float t = Hash01(seed, c, p, s);
            return min + Mathf.FloorToInt(t * (max - min + 1));
        }

        private static float Hash01(int seed, int c, int p, int s)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= 0x9E3779B9u * (uint)(c + 1);
                h ^= 0x85EBCA6Bu * (uint)(p + 1);
                h ^= 0xC2B2AE35u * (uint)(s + 1);
                h = Hash32(h);
                return (h & 0x00FFFFFF) / 16777216f;
            }
        }

        private static uint Hash32(uint x)
        {
            x ^= x >> 16; x *= 0x7FEB352Du;
            x ^= x >> 15; x *= 0x846CA68Bu;
            x ^= x >> 16;
            return x;
        }
        #endregion
    }
}