using UnityEngine;

public static class FloatExtension_InverseLerpUnclamped
{
    public static float InverseLerpUnclamped(this float value, float a, float b)
    {
        if (a != b)
            return (value - a) / (b - a);
        else
            return 0f;
    }

    public static Vector2 InverseLerpUnclamped(this Vector2 value, Vector2 a, Vector2 b)
    {
        return new Vector2(value.x.InverseLerpUnclamped(a.x, b.x),
            value.y.InverseLerpUnclamped(a.y, b.y));
    }

    public static Vector3 InverseLerpUnclamped(this Vector3 value, Vector3 a, Vector3 b)
    {
        return new Vector3(value.x.InverseLerpUnclamped(a.x, b.x),
            value.y.InverseLerpUnclamped(a.y, b.y),
            value.z.InverseLerpUnclamped(a.z, b.z));
    }

    public static Vector4 InverseLerpUnclamped(this Vector4 value, Vector4 a, Vector4 b)
    {
        return new Vector4(value.x.InverseLerpUnclamped(a.x, b.x),
            value.y.InverseLerpUnclamped(a.y, b.y),
            value.z.InverseLerpUnclamped(a.z, b.z),
            value.w.InverseLerpUnclamped(a.w, b.w));
    }

    public static Color InverseLerpUnclamped(this Color value, Color a, Color b)
    {
        return new Color(value.r.InverseLerpUnclamped(a.r, b.r),
            value.g.InverseLerpUnclamped(a.g, b.g),
            value.b.InverseLerpUnclamped(a.b, b.b),
            value.a.InverseLerpUnclamped(a.a, b.a));
    }

    public static Quaternion InverseLerpUnclamped(this Quaternion value, Quaternion a, Quaternion b)
    {
        value.ToAngleAxis(out float vangle, out Vector3 vaxis);
        a.ToAngleAxis(out float aangle, out Vector3 aaxis);
        b.ToAngleAxis(out float bangle, out Vector3 baxis);
        return Quaternion.AngleAxis(
            vangle.InverseLerpUnclamped(aangle, bangle), vaxis.InverseLerpUnclamped(aaxis, baxis));
    }
}
