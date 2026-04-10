using Deepwave.Core;
using System.Collections.Generic;
using UnityEngine;
using Deepwave.ICR.Utilities;

namespace Deepwave.ICR.Data
{
    internal static class SettingsSanitizer
    {
        public const float MinPanelSize = 0.01f;
        private const float MinVerticalTiling = 0.1f;

        public static int SanitizeMinimum(int value, int minimum)
        {
            return Mathf.Max(minimum, value);
        }

        public static float SanitizePositive(float value, float fallback, float minimum)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return fallback;
            }

            return Mathf.Max(minimum, value);
        }

        public static float SanitizeNonNegative(float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return fallback;
            }

            return Mathf.Max(0f, value);
        }

        public static float SanitizePanelLengthVariance(float variance, float panelLength)
        {
            float maxVariance = Mathf.Max(0f, panelLength - MinPanelSize);
            if (float.IsNaN(variance) || float.IsInfinity(variance))
            {
                return 0f;
            }

            return Mathf.Clamp(variance, 0f, maxVariance);
        }

        public static float SanitizeRowOverlap(float overlap, float panelLength)
        {
            float maxOverlap = Mathf.Max(0f, panelLength - MinPanelSize);
            if (float.IsNaN(overlap) || float.IsInfinity(overlap))
            {
                return 0.15f;
            }

            return Mathf.Clamp(overlap, 0f, maxOverlap);
        }

        public static TransformSettings Sanitize(TransformSettings settings, float panelLength)
        {
            settings.horizontalJitterMeters = SanitizeNonNegative(settings.horizontalJitterMeters, 0f);
            settings.verticalGapMeters = SanitizeNonNegative(settings.verticalGapMeters, 0f);
            settings.tiltDegrees = SanitizeNonNegative(settings.tiltDegrees, 0f);
            settings.yawDegrees = SanitizeNonNegative(settings.yawDegrees, 0f);
            settings.lengthVarianceMeters = SanitizePanelLengthVariance(settings.lengthVarianceMeters, panelLength);
            return settings;
        }

        public static NoiseSettings Sanitize(NoiseSettings settings)
        {
            settings.height = SanitizeNonNegative(settings.height, 0.01f);
            settings.warp = SanitizeNonNegative(settings.warp, 0.002f);
            settings.scale = SanitizePositive(settings.scale, 0.35f, 0.01f);
            settings.detail = SanitizeClamped(settings.detail, 0.35f, 0f, 1f);
            return settings;
        }

        public static SplineSettings Sanitize(SplineSettings settings)
        {
#if ICR_SPLINES_SUPPORTED
            if (!settings.enabled)
            {
                settings.spline = null;
            }
#endif
            return settings;
        }

        public static CutSettings Sanitize(CutSettings settings)
        {
            settings.startAngleDegrees = SanitizeFinite(settings.startAngleDegrees, 0f);
            settings.endAngleDegrees = SanitizeFinite(settings.endAngleDegrees, 0f);
            return settings;
        }

        public static TextureSettings Sanitize(TextureSettings settings)
        {
            if (settings.panelColors == null || settings.panelColors.Count == 0)
            {
                settings.panelColors = new List<Color> { Color.white };
            }

            settings.verticalTiling = SanitizePositive(settings.verticalTiling, 1f, MinVerticalTiling);
            settings.verticalOffset = Sanitize(settings.verticalOffset, 0f, 0f, 1f);
            settings.wearLevel = Sanitize(settings.wearLevel, 0, 0, TextureMapper.ColorCount - 1);
            settings.panelColorIndex = Sanitize(settings.panelColorIndex, 0, 0, settings.panelColors.Count - 1);
            return settings;
        }

        public static DynamicFloat Sanitize(DynamicFloat value, float fallback, float min, float max)
        {
            float sanitizedValue = SanitizeClamped(value.value, fallback, min, max);
            value.value = sanitizedValue;

            float rangeMin = SanitizeClamped(value.range.min, sanitizedValue, min, max);
            float rangeMax = SanitizeClamped(value.range.max, sanitizedValue, min, max);
            if (rangeMin > rangeMax) (rangeMin, rangeMax) = (rangeMax, rangeMin);
            value.range = new Vector2Range(rangeMin, rangeMax);

            return value;
        }

        public static DynamicInt Sanitize(DynamicInt value, int fallback, int min, int max)
        {
            int sanitizedValue = SanitizeClamped(value.value, fallback, min, max);
            value.value = sanitizedValue;

            int rangeMin = SanitizeClamped(value.range.min, sanitizedValue, min, max);
            int rangeMax = SanitizeClamped(value.range.max, sanitizedValue, min, max);
            if (rangeMin > rangeMax) (rangeMin, rangeMax) = (rangeMax, rangeMin);
            value.range = new Vector2IntRange(rangeMin, rangeMax);

            return value;
        }

        private static float SanitizeClamped(float value, float fallback, float min, float max)
        {
            float sanitized = SanitizeFinite(value, fallback);
            return Mathf.Clamp(sanitized, min, max);
        }

        private static int SanitizeClamped(int value, int fallback, int min, int max)
        {
            if (value < min || value > max)
            {
                return Mathf.Clamp(value, min, max);
            }

            return value;
        }

        private static float SanitizeFinite(float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return fallback;
            }

            return value;
        }
    }
}
