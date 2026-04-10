using System;
#if ICR_SPLINES_SUPPORTED
using UnityEngine.Splines;
#endif

namespace Deepwave.ICR.Data
{
    public enum SplineMode
    {
        DeformSmooth,
        DeformFlat,
        RigidFollow
    }

    [Serializable]
    public struct SplineSettings
    {
        public bool enabled;
#if ICR_SPLINES_SUPPORTED
        public SplineContainer spline;
#endif
        public SplineMode mode;

        public static SplineSettings Default => new()
        {
            enabled = false,
        };
    }
}
