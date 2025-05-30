using UnityEngine;

public static class VectorExtension_ClosestPointOnSegment
{
    public static Vector3 ClosestPointOnSegment(
        this Vector3 point, Vector3 segment0, Vector3 segment1, out float distance)
    {
        Vector3 dir = segment1 - segment0;
        Vector3 clos = point.ClosestPointOnLine(segment0, segment1, out distance);
        if (distance < 0f)
        {
            distance = 0f;
            clos = segment0;
        }
        else if ((distance * distance) > dir.sqrMagnitude)
        {
            distance = 1f;
            clos = segment1;
        }
        return clos;
    }

    public static Vector3 ClosestPointOnLine(
        this Vector3 point, Vector3 line0, Vector3 line1, out float distance)
    {
        Vector3 normal = (line1 - line0).normalized;

        distance = Vector3.Dot(point - line0, normal) / Vector3.Dot(normal, normal);
        return line0 + (normal * distance);
    }
}
