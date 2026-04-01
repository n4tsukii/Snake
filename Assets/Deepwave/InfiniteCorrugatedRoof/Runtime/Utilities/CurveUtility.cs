using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deepwave.ICR.Utilities
{
    internal static class CurveUtility
    {
        public static AnimationCurve SanitizeCurve(AnimationCurve curve)
        {
            if (curve == null || curve.length < 2)
            {
                return new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(0.2f, 1f),
                    new Keyframe(0.4f, 1f),
                    new Keyframe(0.6f, 0f),
                    new Keyframe(1f, 0f));
            }

            return curve;
        }

        public static void EnsureCurveHas01(AnimationCurve curve)
        {
            if (curve == null || curve.length == 0)
            {
                return;
            }

            var keys = curve.keys;
            bool has0 = Mathf.Abs(keys[0].time - 0f) < 1e-6f;
            bool has1 = Mathf.Abs(keys[^1].time - 1f) < 1e-6f;

            if (!has0)
            {
                curve.AddKey(0f, curve.Evaluate(0f));
            }

            if (!has1)
            {
                curve.AddKey(1f, curve.Evaluate(1f));
            }

            Array.Sort(curve.keys, (a, b) => a.time.CompareTo(b.time));
        }

        public static List<float> GetKeyTimesUsed(AnimationCurve curve, int step)
        {
            step = Mathf.Max(1, step);

            var keys = curve.keys;
            var times = new List<float>(keys.Length + 2)
            {
                0f
            };

            for (int i = 1; i < keys.Length - 1; i++)
            {
                if ((i % step) != 0)
                {
                    continue;
                }

                float t = Mathf.Clamp01(keys[i].time);
                if (t > 0f && t < 1f && Mathf.Abs(times[^1] - t) > 1e-6f)
                {
                    times.Add(t);
                }
            }

            if (Mathf.Abs(times[^1] - 1f) > 1e-6f)
            {
                times.Add(1f);
            }

            times.Sort();
            for (int i = times.Count - 2; i >= 0; i--)
            {
                if (Mathf.Abs(times[i] - times[i + 1]) < 1e-6f)
                {
                    times.RemoveAt(i);
                }
            }

            if (times.Count < 2)
            {
                return new List<float> { 0f, 1f };
            }

            return times;
        }
    }
}
