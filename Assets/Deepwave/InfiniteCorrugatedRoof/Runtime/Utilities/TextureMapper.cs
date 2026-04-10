using System.Runtime.CompilerServices;
using Deepwave.ICR.Data;
using UnityEngine;

namespace Deepwave.ICR.Utilities
{
    internal static class TextureMapper
    {
        public const int ColorCount = 8;
        public const int WavesPerColor = 12;
        public const int TotalWaves = ColorCount * WavesPerColor;

        // Tối ưu phép chia: 1.0 / 9
        public const float InvWavesPerColor = 1f / WavesPerColor;

        // Kích thước 1 Panel trên Atlas (ví dụ 1/8 = 0.125)
        public const float PanelWidthU = 1f / ColorCount;

        // --- HÀM 1: Tính U cục bộ (0.0 -> 1.0) ---
        // Chỉ quan tâm đến vị trí trên 1 panel màu đơn lẻ
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalculateLocalU(int waveIndex, float normalizedT)
        {
            // Công thức: (Index + T) / 9
            // Kết quả luôn nằm trong khoảng [0, 1]
            return ((waveIndex % WavesPerColor) + normalizedT) * InvWavesPerColor;
        }

        // --- HÀM 2: Tính UV cuối cùng (Map vào Atlas) ---
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetUv(
            int texturePanelIndex,
            int textureOrientation,
            float localYRel,
            float verticalOffset,
            in SectionPoint point,
            in TextureSettings settings)
        {
            // B1: Tính U chuẩn hóa (0-1) trên panel hiện tại
            float localU = point.UCoordinate;

            float v = localYRel * settings.verticalTiling / 10 + verticalOffset;

            // Xử lý Orientation
            int orientation = ((textureOrientation % 4) + 4) % 4;

            // Xử lý Mirror U (Lật ngang)
            if (orientation == 2 || orientation == 3)
            {
                localU = 1f - localU;
            }

            // Xử lý Flip V (Lật dọc)
            if (orientation == 1 || orientation == 3)
            {
                v = 1f - v;
            }

            // B3: Chuyển từ Local U sang Atlas Global U
            // Công thức: (PanelIndex + LocalU) * Kích_thước_Panel

            // Clamp panel index an toàn
            int ci = (texturePanelIndex < 0) ? 0 : (texturePanelIndex >= ColorCount ? ColorCount - 1 : texturePanelIndex);

            float globalU = (ci + localU) * PanelWidthU;

            return new Vector2(globalU, v);
        }
    }
}