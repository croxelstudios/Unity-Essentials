using UnityEngine;

public static class VectorExtension_NormalizedScreenUV
{
    public static Vector2 NormalizedScreenUV(this Vector2 source)
    {
        return source.InverseScale(new Vector2(Screen.width, Screen.height));
    }

    public static Vector2 NormalizedScreenUV(this Vector3 source)
    {
        return ((Vector2)source).InverseScale(new Vector2(Screen.width, Screen.height));
    }

    public static Vector2 NormalizedScreenUV(this Vector4 source)
    {
        return ((Vector2)source).InverseScale(new Vector2(Screen.width, Screen.height));
    }
}
