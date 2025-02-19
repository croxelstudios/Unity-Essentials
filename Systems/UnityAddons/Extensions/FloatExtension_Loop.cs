using UnityEngine;
using UnityEngine.UIElements;

public static class FloatExtension_Loop
{
    public static float Loop(this float value, float length)
    {
        return Mathf.Repeat(value, length);
    }

    public static Vector2 Loop(this Vector2 value, Vector2 length)
    {
        return new Vector2(value.x.Loop(length.x), value.y.Loop(length.y));
    }

    public static Vector3 Loop(this Vector3 value, Vector3 length)
    {
        return new Vector3(value.x.Loop(length.x), value.y.Loop(length.y),
            value.z.Loop(length.z));
    }

    public static Vector4 Loop(this Vector4 value, Vector4 length)
    {
        return new Vector4(value.x.Loop(length.x), value.y.Loop(length.y),
            value.z.Loop(length.z), value.w.Loop(length.w));
    }

    public static Vector2 Loop(this Vector2 value, float length)
    {
        return new Vector2(value.x.Loop(length), value.y.Loop(length));
    }

    public static Vector3 Loop(this Vector3 value, float length)
    {
        return new Vector3(value.x.Loop(length), value.y.Loop(length),
            value.z.Loop(length));
    }

    public static Vector4 Loop(this Vector4 value, float length)
    {
        return new Vector4(value.x.Loop(length), value.y.Loop(length),
            value.z.Loop(length), value.w.Loop(length));
    }

    public static float Loop(this float value, float min, float max)
    {
        float rmin = Mathf.Min(min, max);
        float rmax = Mathf.Max(min, max);
        min = rmin;
        max = rmax;

        return Mathf.Repeat(value - min, max - min) + min;
    }

    public static Vector2 Loop(this Vector2 value, Vector2 min, Vector2 max)
    {
        return new Vector2(value.x.Loop(min.x, max.x), value.y.Loop(min.y, max.y));
    }

    public static Vector3 Loop(this Vector3 value, Vector3 min, Vector3 max)
    {
        return new Vector3(value.x.Loop(min.x, max.x), value.y.Loop(min.y, max.y),
            value.z.Loop(min.z, max.z));
    }

    public static Vector4 Loop(this Vector4 value, Vector4 min, Vector4 max)
    {
        return new Vector4(value.x.Loop(min.x, max.x), value.y.Loop(min.y, max.y),
            value.z.Loop(min.z, max.z), value.w.Loop(min.w, max.w));
    }

    public static Vector2 Loop(this Vector2 value, float min, float max)
    {
        return new Vector2(value.x.Loop(min, max), value.y.Loop(min, max));
    }

    public static Vector3 Loop(this Vector3 value, float min, float max)
    {
        return new Vector3(value.x.Loop(min, max), value.y.Loop(min, max),
            value.z.Loop(min, max));
    }

    public static Vector4 Loop(this Vector4 value, float min, float max)
    {
        return new Vector4(value.x.Loop(min, max), value.y.Loop(min, max),
            value.z.Loop(min, max), value.w.Loop(min, max));
    }
}
