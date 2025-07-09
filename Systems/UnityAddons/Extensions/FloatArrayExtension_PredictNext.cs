using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class FloatArrayExtension_PredictNext
{
    static float[] Times(int count)
    {
        float[] times = new float[count];
        for (int i = 0; i < count; i++)
            times[i] = i;
        return times;
    }

    public static float PredictNext(this IEnumerable<float> values, float multiplier = 1f)
    {
        int count = values.Count();
        float[] times = Times(count);
        return PredictNext(values, times, count + multiplier - 1f);
    }

    public static float PredictNext(this IEnumerable<float> values, IEnumerable<float> times, float tNext)
    {
        float[] t = times.ToArray();
        float[] p = values.ToArray();

        int n = t.Length;
        if (n != p.Length || n == 0)
            throw new System.ArgumentException("Lists t & p must have the same length (>0).");

        float result = 0f;

        for (int i = 0; i < n; i++)
        {
            float Li = 1f;
            for (int j = 0; j < n; j++)
            {
                if (j == i) continue;
                float denom = t[i] - t[j];
                Li *= (tNext - t[j]) / denom;
            }

            result += p[i] * Li;
        }

        return result;
    }

    public static Vector2 PredictNext(this IEnumerable<Vector2> values, float multiplier = 1f)
    {
        int count = values.Count();
        float[] times = Times(count);
        return PredictNext(values, times, count + multiplier - 1f);
    }

    public static Vector2 PredictNext(this IEnumerable<Vector2> values, IEnumerable<float> times, float tNext)
    {
        float[] t = times.ToArray();
        Vector2[] p = values.ToArray();

        int n = t.Length;
        if (n != p.Length || n == 0)
            throw new System.ArgumentException("Lists t & p must have the same length (>0).");

        Vector2 result = Vector2.zero;

        for (int i = 0; i < n; i++)
        {
            float Li = 1f;
            for (int j = 0; j < n; j++)
            {
                if (j == i) continue;
                float denom = t[i] - t[j];
                Li *= (tNext - t[j]) / denom;
            }

            result += p[i] * Li;
        }

        return result;
    }

    public static Vector3 PredictNext(this IEnumerable<Vector3> values, float multiplier = 1f)
    {
        int count = values.Count();
        float[] times = Times(count);
        return PredictNext(values, times, count + multiplier - 1f);
    }

    public static Vector3 PredictNext(this IEnumerable<Vector3> values, IEnumerable<float> times, float tNext)
    {
        float[] t = times.ToArray();
        Vector3[] p = values.ToArray();

        int n = t.Length;
        if (n != p.Length || n == 0)
            throw new System.ArgumentException("Lists t & p must have the same length (>0).");

        Vector3 result = Vector3.zero;

        for (int i = 0; i < n; i++)
        {
            float Li = 1f;
            for (int j = 0; j < n; j++)
            {
                if (j == i) continue;
                float denom = t[i] - t[j];
                Li *= (tNext - t[j]) / denom;
            }

            result += p[i] * Li;
        }

        return result;
    }

    public static Vector4 PredictNext(this IEnumerable<Vector4> values, float multiplier = 1f)
    {
        int count = values.Count();
        float[] times = Times(count);
        return PredictNext(values, times, count + multiplier - 1f);
    }

    public static Vector4 PredictNext(this IEnumerable<Vector4> values, IEnumerable<float> times, float tNext)
    {
        float[] t = times.ToArray();
        Vector4[] p = values.ToArray();

        int n = t.Length;
        if (n != p.Length || n == 0)
            throw new System.ArgumentException("Lists t & p must have the same length (>0).");

        Vector4 result = Vector4.zero;

        for (int i = 0; i < n; i++)
        {
            float Li = 1f;
            for (int j = 0; j < n; j++)
            {
                if (j == i) continue;
                float denom = t[i] - t[j];
                Li *= (tNext - t[j]) / denom;
            }

            result += p[i] * Li;
        }

        return result;
    }

    public static Quaternion PredictNext(this IEnumerable<Quaternion> values, float multiplier = 1f)
    {
        int count = values.Count();
        float[] times = Times(count);
        return PredictNext(values, times, count + multiplier - 1f);
    }

    public static Quaternion PredictNext(this IEnumerable<Quaternion> values, IEnumerable<float> times, float tNext)
    {
        float[] t = times.ToArray();
        Quaternion[] p = values.ToArray();

        int n = t.Length;
        if (n != p.Length || n == 0)
            throw new System.ArgumentException("Lists t & p must have the same length (>0).");

        Quaternion result = Quaternion.identity;

        for (int i = 0; i < n; i++)
        {
            float Li = 1f;
            for (int j = 0; j < n; j++)
            {
                if (j == i) continue;
                float denom = t[i] - t[j];
                Li *= (tNext - t[j]) / denom;
            }

            result.Add(p[i].Scale(Li));
        }

        return result;
    }
}
