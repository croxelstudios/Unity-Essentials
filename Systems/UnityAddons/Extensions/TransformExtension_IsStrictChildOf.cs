using UnityEngine;

public static class TransformExtension_IsStrictChildOf
{
    public static bool IsStrictChildOf(this Transform child, Transform parent)
    {
        return (child != parent) && child.IsChildOf(parent);
    }
}
