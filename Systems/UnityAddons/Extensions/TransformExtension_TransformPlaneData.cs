using UnityEngine;

public static class TransformExtension_TransformPlaneData
{
    public static void TransformPlaneData(this Transform transform,
        Vector3 downNormal, Vector3 planeForward, out Vector3 normal, out Vector3 up)
    {
        normal = downNormal;
        up = planeForward;
        if (transform != null)
        {
            normal = transform.rotation * normal;
            up = transform.rotation * up;
            normal = normal.normalized;
            up = up.normalized;
        }
    }
}
