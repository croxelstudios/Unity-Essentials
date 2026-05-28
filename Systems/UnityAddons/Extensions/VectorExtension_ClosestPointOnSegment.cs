using UnityEngine;

public static class VectorExtension_ClosestPointOnSegment
{
    public static Vector2 ClosestPointOnSegment(
        this Vector2 point, Vector2 segment0, Vector2 segment1, out float distance)
    {
        Vector2 dir = segment1 - segment0;
        float sqrMag = dir.sqrMagnitude;

        if (sqrMag <= 0f)
        {
            distance = 0f;
            return segment0;
        }

        Vector2 clos = point.ClosestPointOnLine(segment0, segment1, out distance);
        if (distance < 0f)
        {
            distance = 0f;
            clos = segment0;
        }
        else if ((distance * distance) > sqrMag)
        {
            distance = dir.magnitude;
            clos = segment1;
        }
        return clos;
    }

    public static Vector2 ClosestPointOnLine(
        this Vector2 point, Vector2 line0, Vector2 line1, out float distance)
    {
        Vector2 dir = line1 - line0;
        Vector2 normal = dir.normalized;

        if ((normal.sqrMagnitude < 1f) || (dir.sqrMagnitude <= 0f))
        {
            distance = 0f;
            return line0;
        }

        distance = Vector2.Dot(point - line0, normal) / Vector2.Dot(normal, normal);
        return line0 + (normal * distance);
    }

    public static Vector3 ClosestPointOnSegment(
        this Vector3 point, Vector3 segment0, Vector3 segment1, out float distance)
    {
        Vector3 dir = segment1 - segment0;
        float sqrMag = dir.sqrMagnitude;
        
        if (sqrMag <= 0f)
        {
            distance = 0f;
            return segment0;
        }

        Vector3 clos = point.ClosestPointOnLine(segment0, segment1, out distance);
        if (distance < 0f)
        {
            distance = 0f;
            clos = segment0;
        }
        else if ((distance * distance) > sqrMag)
        {
            distance = dir.magnitude;
            clos = segment1;
        }
        return clos;
    }

    public static Vector3 ClosestPointOnLine(
        this Vector3 point, Vector3 line0, Vector3 line1, out float distance)
    {
        Vector3 dir = line1 - line0;
        Vector3 normal = dir.normalized;

        if ((normal.sqrMagnitude < 1f) || (dir.sqrMagnitude <= 0f))
        {
            distance = 0f;
            return line0;
        }

        distance = Vector3.Dot(point - line0, normal) / Vector3.Dot(normal, normal);
        return line0 + (normal * distance);
    }

    public static Vector4 ClosestPointOnSegment(
        this Vector4 point, Vector4 segment0, Vector4 segment1, out float distance)
    {
        Vector4 dir = segment1 - segment0;
        float sqrMag = dir.sqrMagnitude;

        if (sqrMag <= 0f)
        {
            distance = 0f;
            return segment0;
        }

        Vector4 clos = point.ClosestPointOnLine(segment0, segment1, out distance);
        if (distance < 0f)
        {
            distance = 0f;
            clos = segment0;
        }
        else if ((distance * distance) > sqrMag)
        {
            distance = dir.magnitude;
            clos = segment1;
        }
        return clos;
    }

    public static Vector4 ClosestPointOnLine(
        this Vector4 point, Vector4 line0, Vector4 line1, out float distance)
    {
        Vector4 dir = line1 - line0;
        Vector4 normal = dir.normalized;

        if ((normal.sqrMagnitude < 1f) || (dir.sqrMagnitude <= 0f))
        {
            distance = 0f;
            return line0;
        }

        distance = Vector4.Dot(point - line0, normal) / Vector4.Dot(normal, normal);
        return line0 + (normal * distance);
    }
}
