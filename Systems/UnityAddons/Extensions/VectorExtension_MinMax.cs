using UnityEngine;

public static class VectorExtension_MinMax
{
    public static float Min(this float value, float min)
    {
        return Mathf.Min(value, min);
    }

    public static float Max(this float value, float max)
    {
        return Mathf.Max(value, max);
    }

    public static Vector2 Min(this Vector2 value, Vector2 min)
    {
        return Vector2.Min(value, min);
    }

    public static Vector2 Max(this Vector2 value, Vector2 max)
    {
        return Vector2.Max(value, max);
    }

    public static Vector2 Min(this Vector2 value, float min)
    {
        return value.Min(new Vector2(min, min));
    }

    public static Vector2 Max(this Vector2 value, float max)
    {
        return value.Max(new Vector2(max, max));
    }

    public static Vector3 Min(this Vector3 value, Vector3 min)
    {
        return Vector3.Min(value, min);
    }

    public static Vector3 Max(this Vector3 value, Vector3 max)
    {
        return Vector3.Max(value, max);
    }

    public static Vector3 Min(this Vector3 value, float min)
    {
        return value.Min(new Vector3(min, min, min));
    }

    public static Vector3 Max(this Vector3 value, float max)
    {
        return value.Max(new Vector3(max, max, max));
    }

    public static Vector4 Min(this Vector4 value, Vector4 min)
    {
        return Vector4.Min(value, min);
    }

    public static Vector4 Max(this Vector4 value, Vector4 max)
    {
        return Vector4.Max(value, max);
    }

    public static Vector4 Min(this Vector4 value, float min)
    {
        return value.Min(new Vector4(min, min, min, min));
    }

    public static Vector4 Max(this Vector4 value, float max)
    {
        return value.Max(new Vector4(max, max, max, max));
    }

    public static Color Min(this Color value, Color min)
    {
        return new Color(
            Mathf.Min(value.r, min.r),
            Mathf.Min(value.g, min.g),
            Mathf.Min(value.b, min.b),
            Mathf.Min(value.a, min.a)
            );
    }

    public static Color Max(this Color value, Color max)
    {
        return new Color(
            Mathf.Max(value.r, max.r),
            Mathf.Max(value.g, max.g),
            Mathf.Max(value.b, max.b),
            Mathf.Max(value.a, max.a)
            );
    }

    public static Color Min(this Color value, float min)
    {
        return value.Min(new Color(min, min, min, min));
    }

    public static Color Max(this Color value, float max)
    {
        return value.Max(new Color(max, max, max, max));
    }
}
