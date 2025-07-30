using System.Collections.Generic;
using UnityEngine;

public static class VectorCollectionExtension_Average
{
    public static Vector3 Average(this IEnumerable<Vector3> collection)
    {
        Vector3 pos = Vector3.zero;
        int count = 0;
        foreach (Vector3 item in collection)
        {
            pos += item;
            count++;
        }

        return pos / count;
    }

    public static Vector2 Average(this IEnumerable<Vector2> collection)
    {
        Vector2 pos = Vector2.zero;
        int count = 0;
        foreach (Vector2 item in collection)
        {
            pos += item;
            count++;
        }

        return pos / count;
    }

    public static Vector4 Average(this IEnumerable<Vector4> collection)
    {
        Vector4 pos = Vector4.zero;
        int count = 0;
        foreach (Vector4 item in collection)
        {
            pos += item;
            count++;
        }

        return pos / count;
    }

    public static float Average(this IEnumerable<float> collection)
    {
        float pos = 0f;
        int count = 0;
        foreach (float item in collection)
        {
            pos += item;
            count++;
        }

        return pos / count;
    }

    public static int Average(this IEnumerable<int> collection)
    {
        int pos = 0;
        int count = 0;
        foreach (int item in collection)
        {
            pos += item;
            count++;
        }

        return pos / count;
    }
}
