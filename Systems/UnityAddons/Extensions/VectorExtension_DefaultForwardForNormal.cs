using UnityEngine;

public static class VectorExtension_DefaultForwardForNormal
{
    public static Vector3 DefaultForwardForNormal(this Vector3 normal)
    {
        normal = normal.normalized;

        Vector3 dir;
        if (Vector3.Cross(normal, Vector3.forward).sqrMagnitude <= 0f) dir = Vector3.up;
        else dir = Vector3.forward;

        return Vector3.ProjectOnPlane(dir, normal).normalized;
    }
}
