using UnityEngine;

public static class VectorExtension_ComponentLerp
{
    public static Vector2 ComponentLerp(this Vector2 value, Vector2 a, Vector2 b)
    {
        return new Vector2(Mathf.LerpUnclamped(a.x, b.x, value.x),
            Mathf.LerpUnclamped(a.y, b.y, value.y));
    }

    public static Vector3 ComponentLerp(this Vector3 value, Vector3 a, Vector3 b)
    {
        return new Vector3(Mathf.LerpUnclamped(a.x, b.x, value.x),
            Mathf.LerpUnclamped(a.y, b.y, value.y),
            Mathf.LerpUnclamped(a.z, b.z, value.z));
    }

    public static Vector4 ComponentLerp(this Vector4 value, Vector4 a, Vector4 b)
    {
        return new Vector4(Mathf.LerpUnclamped(a.x, b.x, value.x),
            Mathf.LerpUnclamped(a.y, b.y, value.y),
            Mathf.LerpUnclamped(a.z, b.z, value.z),
            Mathf.LerpUnclamped(a.w, b.w, value.w));
    }

    public static Color ComponentLerp(this Color value, Color a, Color b)
    {
        return new Vector4(Mathf.LerpUnclamped(a.r, b.r, value.r),
            Mathf.LerpUnclamped(a.g, b.g, value.g),
            Mathf.LerpUnclamped(a.b, b.b, value.b),
            Mathf.LerpUnclamped(a.a, b.a, value.a));
    }

    public static Quaternion ComponentLerp(this Quaternion value, Quaternion a, Quaternion b)
    {
        value.ToAngleAxis(out float vangle, out Vector3 vaxis);
        a.ToAngleAxis(out float aangle, out Vector3 aaxis);
        b.ToAngleAxis(out float bangle, out Vector3 baxis);
        return Quaternion.AngleAxis(
            Mathf.LerpUnclamped(aangle, bangle, vangle), vaxis.ComponentLerp(aaxis, baxis));
    }
}
