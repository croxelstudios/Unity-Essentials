using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloatExtension_MapToVector
{
    public static Vector2 MapToVector(this float seed)
    {
        seed = Mathf.Clamp01(Mathf.Abs(seed));
        Vector2 result = Vector2.zero;
        int i = 1;
        while ((seed > 0) && (i < 39))
        {
            seed *= 10f;
            i++;
            float trunk = Mathf.Floor(seed);
            seed -= trunk;
            result.x += trunk * Mathf.Pow(0.1f, i / 2);

            seed *= 10f;
            i++;
            trunk = Mathf.Floor(seed);
            seed -= trunk;
            result.y += trunk * Mathf.Pow(0.1f, i / 2);
        }
        return result;
    }
}
