using UnityEngine;

public static class VectorExtension_GetPerpendicular
{
    public static Vector3 GetPerpendicular(this Vector3 normal)
    {
        normal.Normalize();

        // Choose a helper vector that is unlikely to be parallel.
        Vector3 helper =
            Mathf.Abs(normal.y) < 0.999f
            ? Vector3.up
            : Vector3.right;

        return Vector3.Cross(normal, helper).normalized;
    }
}
