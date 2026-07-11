using UnityEditor;
using UnityEngine;

public static class BoundsExtension_Volume
{
    public static float Volume(this Bounds bounds, bool is2D = false)
    {
        return bounds.size.x * bounds.size.y * (is2D ? 1f : bounds.size.z);
    }
}
