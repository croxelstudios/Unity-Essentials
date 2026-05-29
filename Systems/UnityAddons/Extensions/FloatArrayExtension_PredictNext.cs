using System.Collections.Generic;
using System.Linq;

public static class FloatArrayExtension_PredictNext
{
    static float[] DefaultTimes(int count)
    {
        float[] times = new float[count];
        for (int i = 0; i < count; i++)
            times[i] = i;
        return times;
    }

    public static T PredictNext<T>(this IEnumerable<T> values, float multiplier = 1f)
    {
        int count = values.Count();
        float[] times = DefaultTimes(count);
        return PredictNext(values, times, count + multiplier - 1f);
    }

    public static T PredictNext<T>(this IEnumerable<T> values, IEnumerable<float> times, float tNext)
    {
        float[] t = times.ToArray();
        T[] p = values.ToArray();

        int n = t.Length;
        if (n != p.Length || n == 0)
            throw new System.ArgumentException("Lists t & p must have the same length (>0).");

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
}
