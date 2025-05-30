using UnityEngine;

public static class VectorExtension_InverseScale
{
    public static Vector2 InverseScale(this Vector2 dividend, Vector2 divisor)
    {
        divisor = new Vector2(divisor.x == 0 ? Mathf.Epsilon : divisor.x,
            divisor.y == 0 ? Mathf.Epsilon : divisor.y);
        Vector3 inversedDivisor = new Vector3(1 / divisor.x, 1 / divisor.y);
        dividend.Scale(inversedDivisor);
        return dividend;
    }

    public static Vector3 InverseScale(this Vector3 dividend, Vector3 divisor)
    {
        divisor = new Vector3(divisor.x == 0 ? Mathf.Epsilon : divisor.x,
            divisor.y == 0 ? Mathf.Epsilon : divisor.y,
            divisor.z == 0 ? Mathf.Epsilon : divisor.z);
        Vector3 inversedDivisor = new Vector3(1 / divisor.x, 1 / divisor.y, 1 / divisor.z);
        dividend.Scale(inversedDivisor);
        return dividend;
    }

    public static Vector4 InverseScale(this Vector4 dividend, Vector4 divisor)
    {
        divisor = new Vector4(divisor.x == 0 ? Mathf.Epsilon : divisor.x,
            divisor.y == 0 ? Mathf.Epsilon : divisor.y,
            divisor.z == 0 ? Mathf.Epsilon : divisor.z,
            divisor.w == 0 ? Mathf.Epsilon : divisor.w);
        Vector4 inversedDivisor = new Vector4(1 / divisor.x, 1 / divisor.y, 1 / divisor.z, 1 / divisor.w);
        dividend.Scale(inversedDivisor);
        return dividend;
    }
}
