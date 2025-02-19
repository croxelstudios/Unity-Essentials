using UnityEngine;

public static class VectorExtension_CodifyToColor
{
    public static Color CodifyToColor(this Vector3 v)
    {
        float n = Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        string s = string.Format("{0:F0}", n);
        int p = s == "0" ? 0 : s.Length;
        return CodifyToColor(v, p);
    }

    public static Color CodifyToColor(this Vector2 v)
    {
        return ((Vector3)v).CodifyToColor();
    }

    public static Color CodifyToColor(this Vector3 v, int precision)
    {
        float p = 1 / Mathf.Pow(10, precision);
        v = v * p;
        v += Vector3.one;
        v *= 0.5f;
        return new Color(v.x, v.y, v.z, p);
    }

    public static Color CodifyToColor(this Vector2 v, int precision)
    {
        return ((Vector3)v).CodifyToColor(precision);
    }
}
