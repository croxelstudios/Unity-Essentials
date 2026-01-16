using UnityEngine;

public static class BoundsExtension_MaxExtentSize
{
    public static float MaxExtent(this Bounds bounds)
    {
        return Mathf.Max(bounds.extents.x, Mathf.Max(bounds.extents.y, bounds.extents.z));
    }

    public static float MaxSize(this Bounds bounds)
    {
        return Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
    }
}
