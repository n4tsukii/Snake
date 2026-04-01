using Deepwave.ICR.Data;
using UnityEngine;

namespace Deepwave.ICR.Utilities
{
    internal static class CutUtility
    {
        public static Vector3 ApplyCut(
            Vector3 position,
            CutSettings cuts,
            float originX,
            float originY,
            float totalWidth,
            float totalHeight,
            float panelOriginX,
            float panelOriginY,
            float panelWidth,
            float panelLength)
        {
            if (!cuts.enabled)
            {
                return position;
            }

            float yStart;
            float yEnd;

            if (cuts.mode == CutMode.Horizontal)
            {
                float panelXMin = panelOriginX;
                float panelXMax = panelOriginX + panelWidth;

                float startA = GetStartY(panelXMin, cuts, originX, originY, totalWidth);
                float startB = GetStartY(panelXMax, cuts, originX, originY, totalWidth);
                yStart = Mathf.Min(startA, startB);

                float endA = GetEndY(panelXMin, cuts, originX, originY, totalWidth, totalHeight);
                float endB = GetEndY(panelXMax, cuts, originX, originY, totalWidth, totalHeight);
                yEnd = Mathf.Min(endA, endB);
            }
            else
            {
                yStart = GetStartY(position.x, cuts, originX, originY, totalWidth);
                yEnd = GetEndY(position.x, cuts, originX, originY, totalWidth, totalHeight);
            }

            if (yStart > yEnd)
            {
                float mid = (yStart + yEnd) * 0.5f;
                yStart = mid;
                yEnd = mid;
            }

            position.y = Mathf.Clamp(position.y, yStart, yEnd);
            return position;
        }

        private static float GetStartY(float x, CutSettings cuts, float originX, float originY, float totalWidth)
        {
            float xFromLeft = x - originX;
            float xFromRight = (originX + totalWidth) - x;
            float startBase = cuts.startAngleDegrees >= 0f ? xFromLeft : xFromRight;
            float startOffset = Mathf.Tan(Mathf.Abs(cuts.startAngleDegrees) * Mathf.Deg2Rad) * startBase;
            return originY + startOffset;
        }

        private static float GetEndY(float x, CutSettings cuts, float originX, float originY, float totalWidth, float totalHeight)
        {
            float xFromLeft = x - originX;
            float xFromRight = (originX + totalWidth) - x;
            float endBase = cuts.endAngleDegrees >= 0f ? xFromLeft : xFromRight;
            float endOffset = Mathf.Tan(Mathf.Abs(cuts.endAngleDegrees) * Mathf.Deg2Rad) * endBase;
            return (originY + totalHeight) - endOffset;
        }
    }
}
