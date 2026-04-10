using Deepwave.ICR.Data;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Deepwave.ICR.Utilities
{
    public readonly struct LodProfile
    {
        public readonly int KeyframeStep;
        public readonly int LengthSegmentsPerPanel;
        public readonly bool IsLowestLOD;

        public LodProfile(
            int keyframeStep,
            int lengthSegmentsPerPanel,
            bool isLowestLOD)
        {
            KeyframeStep = keyframeStep;
            LengthSegmentsPerPanel = lengthSegmentsPerPanel;
            IsLowestLOD = isLowestLOD;
        }
    }

    public static class LodUtility
    {
        // Cache mảng static readonly (An toàn và nhanh)
        private static readonly float[] LodScreenRelativeHeights = { 0.85f, 0.45f, 0.01f };

        public static int LodCount => LodScreenRelativeHeights.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetScreenRelativeHeight(int lodIndex)
        {
            int safeIndex = (lodIndex < 0) ? 0 : (lodIndex >= LodCount ? LodCount - 1 : lodIndex);
            return LodScreenRelativeHeights[safeIndex];
        }

        internal static LodProfile GetProfile(BuildSettings settings, int lodIndex)
        {
            // Đảm bảo index luôn hợp lệ trước khi xử lý
            lodIndex = Mathf.Clamp(lodIndex, 0, LodCount - 1);

            int keyframeStep = lodIndex switch
            {
                0 => 1,
                1 => 2,
                _ => 4
            };

            // "val >> n" tương đương "val / 2^n" nhưng nhanh hơn nhiều và không tạo rác GC.
            // Ví dụ: LOD 0 (>>0) giữ nguyên, LOD 1 (>>1) chia 2, LOD 2 (>>2) chia 4.
            int lengthSegments = Mathf.Max(1, settings.LengthSegmentsPerPanel >> lodIndex);

            bool isLowestLOD = lodIndex == LodCount - 1;

            return new LodProfile(
                keyframeStep,
                lengthSegments,
                isLowestLOD);
        }
    }
}