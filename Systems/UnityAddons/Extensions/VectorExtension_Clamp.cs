using UnityEngine;

public static class VectorExtension_Clamp
{
    public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max)
    {
        return new Vector2(
            Mathf.Clamp(vector.x, min.x, max.x),
            Mathf.Clamp(vector.y, min.y, max.y));
    }

    public static Vector2 Clamp(this Vector2 vector, float min, float max)
    {
        return vector.Clamp(Vector2.one * min, Vector2.one * max);
    }

    public static Vector2 Clamp(this Vector2 vector, Vector2 min, float max)
    {
        return vector.Clamp(min, Vector2.one * max);
    }

    public static Vector2 Clamp(this Vector2 vector, float min, Vector2 max)
    {
        return vector.Clamp(Vector2.one * min, max);
    }

    public static Vector2 Clamp01(this Vector2 vector)
    {
        return vector.Clamp(0f, 1f);
    }

    public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
    {
        return new Vector3(
            Mathf.Clamp(vector.x, min.x, max.x),
            Mathf.Clamp(vector.y, min.y, max.y),
            Mathf.Clamp(vector.z, min.z, max.z));
    }

    public static Vector3 Clamp(this Vector3 vector, float min, float max)
    {
        return vector.Clamp(Vector3.one * min, Vector3.one * max);
    }

    public static Vector3 Clamp(this Vector3 vector, Vector3 min, float max)
    {
        return vector.Clamp(min, Vector3.one * max);
    }

    public static Vector3 Clamp(this Vector3 vector, float min, Vector3 max)
    {
        return vector.Clamp(Vector3.one * min, max);
    }

    public static Vector3 Clamp01(this Vector3 vector)
    {
        return vector.Clamp(0f, 1f);
    }

    public static Vector4 Clamp(this Vector4 vector, Vector4 min, Vector4 max)
    {
        return new Vector4(
            Mathf.Clamp(vector.x, min.x, max.x),
            Mathf.Clamp(vector.y, min.y, max.y),
            Mathf.Clamp(vector.z, min.z, max.z),
            Mathf.Clamp(vector.w, min.w, max.w));
    }

    public static Vector4 Clamp(this Vector4 vector, float min, float max)
    {
        return vector.Clamp(Vector4.one * min, Vector4.one * max);
    }

    public static Vector4 Clamp(this Vector4 vector, Vector4 min, float max)
    {
        return vector.Clamp(min, Vector4.one * max);
    }

    public static Vector4 Clamp(this Vector4 vector, float min, Vector4 max)
    {
        return vector.Clamp(Vector4.one * min, max);
    }

    public static Vector4 Clamp01(this Vector4 vector)
    {
        return vector.Clamp(0f, 1f);
    }

    public static Color Clamp(this Color color, Color min, Color max)
    {
        return new Color(
            Mathf.Clamp(color.r, min.r, max.r),
            Mathf.Clamp(color.g, min.g, max.g),
            Mathf.Clamp(color.b, min.b, max.b),
            Mathf.Clamp(color.a, min.a, max.a));
    }

    public static Color Clamp(this Color color, float min, float max)
    {
        return color.Clamp(Color.white * min, Color.white * max);
    }

    public static Color Clamp(this Color color, Color min, float max)
    {
        return color.Clamp(min, Color.white * max);
    }

    public static Color Clamp(this Color color, float min, Color max)
    {
        return color.Clamp(Color.white * min, max);
    }

    public static Color Clamp01(this Color color)
    {
        return color.Clamp(0f, 1f);
    }
}
