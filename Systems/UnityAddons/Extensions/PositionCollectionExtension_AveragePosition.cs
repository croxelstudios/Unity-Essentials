using System.Collections.Generic;
using UnityEngine;

public static class PositionCollectionExtension_AveragePosition
{
    public static Vector3 AveragePosition<T>(this IEnumerable<T> collection) where T : Object
    {
        Vector3 pos = Vector3.zero;
        int count = 0;
        foreach (T item in collection)
        {
            Transform tr = item.GetTransform();
            pos += tr.position;
            count++;
        }

        return pos / count;
    }
}
