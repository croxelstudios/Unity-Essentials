using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ValueArrayExtension_PredictNext
{
    static float[] DefaultTimes(int count)
    {
        float[] times = new float[count];
        for (int i = 0; i < count; i++)
            times[i] = i;
        return times;
    }

    public static T PredictNext<T>(IEnumerable<T> values, float multiplier = 1f)
    {
        int count = values.Count();
        float[] times = DefaultTimes(count);
        return PredictNext(values, times, count + multiplier - 1f);
    }

    public static T PredictNext<T>(IEnumerable<T> values, IEnumerable<float> times, float tNext)
    {
        float[] t = times.ToArray();
        T[] p = values.ToArray();

        int n = t.Length;
        if (n != p.Length)
            throw new System.ArgumentException("Values and times lists must have the same length (>0).");

        T result = Default<T>.Value;

        for (int i = 0; i < n; i++)
        {
            float Li = 1f;
            for (int j = 0; j < n; j++)
            {
                if (j == i) continue;
                float denom = t[i] - t[j];
                Li *= (tNext - t[j]) / denom;
            }

            result = Generics.Add(result, Generics.Scale(p[i], Li));
        }

        return result;
    }

    public static float PredictNext(this IEnumerable<float> values, float multiplier = 1f)
    {
        return PredictNext<float>(values, multiplier);
    }

    public static float PredictNext(this IEnumerable<float> values, IEnumerable<float> times, float tNext)
    {
        return PredictNext<float>(values, times, tNext);
    }

    public static Vector2 PredictNext(this IEnumerable<Vector2> values, float multiplier = 1f)
    {
        return PredictNext<Vector2>(values, multiplier);
    }

    public static Vector2 PredictNext(this IEnumerable<Vector2> values, IEnumerable<float> times, float tNext)
    {
        return PredictNext<Vector2>(values, times, tNext);
    }

    public static Vector3 PredictNext(this IEnumerable<Vector3> values, float multiplier = 1f)
    {
        return PredictNext<Vector3>(values, multiplier);
    }

    public static Vector3 PredictNext(this IEnumerable<Vector3> values, IEnumerable<float> times, float tNext)
    {
        return PredictNext<Vector3>(values, times, tNext);
    }

    public static Vector4 PredictNext(this IEnumerable<Vector4> values, float multiplier = 1f)
    {
        return PredictNext<Vector4>(values, multiplier);
    }

    public static Vector4 PredictNext(this IEnumerable<Vector4> values, IEnumerable<float> times, float tNext)
    {
        return PredictNext<Vector4>(values, times, tNext);
    }

    public static Color PredictNext(this IEnumerable<Color> values, float multiplier = 1f)
    {
        return PredictNext<Color>(values, multiplier);
    }

    public static Color PredictNext(this IEnumerable<Color> values, IEnumerable<float> times, float tNext)
    {
        return PredictNext<Color>(values, times, tNext);
    }

    public static Quaternion PredictNext(this IEnumerable<Quaternion> values, float multiplier = 1f)
    {
        return PredictNext<Quaternion>(values, multiplier);
    }

    public static Quaternion PredictNext(this IEnumerable<Quaternion> values, IEnumerable<float> times, float tNext)
    {
        return PredictNext<Quaternion>(values, times, tNext);
    }
}
