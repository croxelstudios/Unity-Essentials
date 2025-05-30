using UnityEngine;

public static class FloatExtension_Reciprocal
{
    public static float Reciprocal(this float value)
    {
        return (value != 0f) ? 1f / value : Mathf.Infinity;
    }

    public static Vector2 Reciprocal(this Vector2 value)
    {
        return new Vector2(value.x.Reciprocal(), value.y.Reciprocal());
    }

    public static Vector3 Reciprocal(this Vector3 value)
    {
        return new Vector3(value.x.Reciprocal(), value.y.Reciprocal(), value.z.Reciprocal());
    }

    public static Vector4 Reciprocal(this Vector4 value)
    {
        return new Vector4(value.x.Reciprocal(), value.y.Reciprocal(), value.z.Reciprocal(), value.w.Reciprocal());
    }

    public static Color Reciprocal(this Color value)
    {
        return new Color(value.r.Reciprocal(), value.g.Reciprocal(), value.b.Reciprocal(), value.a.Reciprocal());
    }
}
