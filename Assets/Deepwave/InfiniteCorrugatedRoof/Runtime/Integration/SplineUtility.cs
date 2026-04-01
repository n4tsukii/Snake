#if ICR_SPLINES_SUPPORTED
using UnityEngine.Splines;
#endif

using Deepwave.ICR.Data;
using Unity.Mathematics;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Deepwave.ICR.Integration
{
    internal static class SplineUtility
    {
        internal readonly struct SplineCache
        {
            public readonly bool IsValid;
#if ICR_SPLINES_SUPPORTED
            public readonly SplineContainer Container;
            public readonly Matrix4x4 LocalToWorld;
            public readonly Matrix4x4 WorldToLocal;
            public readonly float OriginX;
            public readonly float InvLength;

            public SplineCache(SplineContainer container, Transform sheet, float originX)
            {
                if (container != null && container.Spline != null && sheet != null)
                {
                    Container = container;
                    LocalToWorld = container.transform.localToWorldMatrix;
                    WorldToLocal = sheet.worldToLocalMatrix;
                    OriginX = originX;

                    float len = container.CalculateLength();
                    InvLength = len > 1e-4f ? 1f / len : 0f;
                    IsValid = true;
                }
                else
                {
                    this = default;
                }
            }
#else
            // Empty constructor for when Splines is not supported
            public SplineCache(object container, Transform sheet, float originX)
            {
                this = default;
            }
#endif
        }

        public static SplineCache BuildCache(SplineSettings settings, Transform sheet, float originX)
        {
#if ICR_SPLINES_SUPPORTED
            return (settings.enabled && settings.spline != null)
                ? new SplineCache(settings.spline, sheet, originX)
                : default;
#else
            return default;
#endif
        }

        public static Vector3 ApplySpline(Vector3 localPos, in SplineCache cache, SplineMode mode, float panelStartX, float panelWidth)
        {
#if ICR_SPLINES_SUPPORTED
            if (!cache.IsValid) return localPos;

            float centerX = panelStartX + (panelWidth * 0.5f);
            float sampleX = mode == SplineMode.RigidFollow ? centerX : localPos.x;

            float t = math.saturate((sampleX - cache.OriginX) * cache.InvLength);

            cache.Container.Spline.Evaluate(t, out float3 sPos, out float3 sTan, out _);

            Vector3 worldPos = cache.LocalToWorld.MultiplyPoint3x4(sPos);
            Vector3 worldTan = cache.LocalToWorld.MultiplyVector(sTan);

            Vector3 axisTan = NormalizeSafe(new Vector3(worldTan.x, 0f, worldTan.z), Vector3.forward);
            Vector3 axisUp = Vector3.up;
            Vector3 axisRight = Vector3.Cross(axisUp, axisTan).normalized;


            if (mode == SplineMode.DeformFlat)
            {
                float tCenter = math.saturate((centerX - cache.OriginX) * cache.InvLength);
                float3 centerPos = cache.Container.Spline.EvaluatePosition(tCenter);
                worldPos.y = cache.LocalToWorld.MultiplyPoint3x4(centerPos).y;
            }

            Vector3 finalWorldPos = worldPos;

            if (mode == SplineMode.RigidFollow)
            {
                finalWorldPos += axisTan * (localPos.x - centerX);
            }

            finalWorldPos += (axisRight * localPos.z) + (axisUp * localPos.y);

            return cache.WorldToLocal.MultiplyPoint3x4(finalWorldPos);
#else
            return localPos;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 NormalizeSafe(Vector3 v, Vector3 fallback)
        {
            return v.sqrMagnitude > 1e-6f ? v.normalized : fallback;
        }
    }
}