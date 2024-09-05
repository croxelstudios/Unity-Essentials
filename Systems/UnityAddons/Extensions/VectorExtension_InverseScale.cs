using UnityEngine;

public static class VectorExtension_InverseScale
{
    public static Vector3 InverseScale(this Vector3 dividend, Vector3 divisor)
    {
        divisor = new Vector3(divisor.x == 0 ? Mathf.Epsilon : divisor.x,
            divisor.y == 0 ? Mathf.Epsilon : divisor.y,
            divisor.z == 0 ? Mathf.Epsilon : divisor.z);
        Vector3 inversedDivisor = new Vector3(1 / divisor.x, 1 / divisor.y, 1 / divisor.z);
        dividend.Scale(inversedDivisor);
        return dividend;
    }
}
