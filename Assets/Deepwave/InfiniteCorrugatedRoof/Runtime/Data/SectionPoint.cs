using UnityEngine;

namespace Deepwave.ICR.Data
{
    internal readonly struct SectionPoint
    {
        public readonly Vector2 Position;
        public readonly float UCoordinate;

        public SectionPoint(Vector2 position, float uCoordinate)
        {
            Position = position;
            UCoordinate = uCoordinate;
        }
    }

}
