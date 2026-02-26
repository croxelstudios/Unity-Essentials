using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension_SiblingIndexPath
{
    public static int[] SiblingIndexPath(this Transform t)
    {
        var parts = new List<int>();
        while (t != null)
        {
            parts.Add(t.GetSiblingIndex());
            t = t.parent;
        }
        parts.Reverse();
        return parts.ToArray();
    }

    public static string SiblingIndexPathString(this Transform t)
    {
        return string.Join(".", t.SiblingIndexPath());
    }
}
