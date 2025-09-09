using UnityEngine;

public static class FloatExtension_Remap
{
    public static float Remap(this float value, float origA, float origB, float targA, float targB)
    {
        return Mathf.LerpUnclamped(targA, targB, value.InverseLerpUnclamped(origA, origB));
    }

    public static Vector2 Remap(this Vector2 value,
        Vector2 origA, Vector2 origB, Vector2 targA, Vector2 targB)
    {
        return value.InverseLerpUnclamped(origA, origB).ComponentLerp(targA, targB);
    }

    public static Vector3 Remap(this Vector3 value,
        Vector3 origA, Vector3 origB, Vector3 targA, Vector3 targB)
    {
        return value.InverseLerpUnclamped(origA, origB).ComponentLerp(targA, targB);
    }

    public static Vector4 Remap(this Vector4 value,
        Vector4 origA, Vector4 origB, Vector4 targA, Vector4 targB)
    {
        return value.InverseLerpUnclamped(origA, origB).ComponentLerp(targA, targB);
    }

    public static Color Remap(this Color value,
        Color origA, Color origB, Color targA, Color targB)
    {
        return value.InverseLerpUnclamped(origA, origB).ComponentLerp(targA, targB);
    }

    public static Quaternion Remap(this Quaternion value,
        Quaternion origA, Quaternion origB, Quaternion targA, Quaternion targB)
    {
        return value.InverseLerpUnclamped(origA, origB).ComponentLerp(targA, targB);
    }
}
