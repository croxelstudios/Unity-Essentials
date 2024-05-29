using UnityEngine;

public static class VectorExtension_Mirror
{
    public static Vector3 Mirror(this Vector3 point, Vector3 planeNormal, Vector3 planePoint)
    {
        float planeDot = Vector3.Dot(planePoint, planeNormal);
        float externalDot = Vector3.Dot(point, planeNormal);
        float planeSquareDot = Vector3.Dot(planeNormal, planeNormal);
        float factor = (externalDot - planeDot) / planeSquareDot;
        return point - (planeNormal * factor * 2f);
    }

    public static Vector2 Mirror(this Vector2 point, Vector2 planeNormal, Vector2 planePoint)
    {
        float planeDot = Vector3.Dot(planePoint, planeNormal);
        float externalDot = Vector3.Dot(point, planeNormal);
        float planeSquareDot = Vector3.Dot(planeNormal, planeNormal);
        float factor = (externalDot - planeDot) / planeSquareDot;
        return point - (planeNormal * factor * 2f);
    }
}
