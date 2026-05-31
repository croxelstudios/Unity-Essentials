using UnityEngine;

public static class VectorExtension_ClosestPointOnSegment
{
    public static T ClosestPointOnSegment<T>(
        T point, T segment0, T segment1, out float distance)
    {
        T dir = Generics.Subtract(segment1, segment0);
        if (!Generics.HasMagnitude(dir))
        {
            distance = 0f;
            return segment0;
        }

        T clos = ClosestPointOnLine(point, segment0, segment1, out distance);
        if (distance < 0f)
        {
            distance = 0f;
            clos = segment0;
        }
        else if (!Generics.MagnitudeGreaterThan(dir, distance, true))
        {
            distance = Generics.Magnitude(dir);
            clos = segment1;
        }
        return clos;
    }

    public static T ClosestPointOnLine<T>(
        T point, T line0, T line1, out float distance)
    {
        T dir = Generics.Subtract(line1, line0);
        T normal = Generics.Direction<T, T>(dir);

        if (!(Generics.HasMagnitude(dir) && Generics.MagnitudeGreaterThan(normal, 1f, true)))
        {
            distance = 0f;
            return line0;
        }

        distance = Generics.Dot(Generics.Subtract(point, line0), normal) / Generics.Dot(normal, normal);
        return Generics.Add(line0, Generics.Scale(normal, distance));
    }

    public static Vector2 ClosestPointOnSegment(
        this Vector2 point, Vector2 segment0, Vector2 segment1, out float distance)
    {
        return ClosestPointOnSegment<Vector2>(point, segment0, segment1, out distance);
    }

    public static Vector2 ClosestPointOnLine(
        this Vector2 point, Vector2 line0, Vector2 line1, out float distance)
    {
        return ClosestPointOnLine<Vector2>(point, line0, line1, out distance);
    }

    public static Vector3 ClosestPointOnSegment(
        this Vector3 point, Vector3 segment0, Vector3 segment1, out float distance)
    {
        return ClosestPointOnSegment<Vector3>(point, segment0, segment1, out distance);
    }

    public static Vector3 ClosestPointOnLine(
        this Vector3 point, Vector3 line0, Vector3 line1, out float distance)
    {
        return ClosestPointOnLine<Vector3>(point, line0, line1, out distance);
    }

    public static Vector4 ClosestPointOnSegment(
        this Vector4 point, Vector4 segment0, Vector4 segment1, out float distance)
    {
        return ClosestPointOnSegment<Vector4>(point, segment0, segment1, out distance);
    }

    public static Vector4 ClosestPointOnLine(
        this Vector4 point, Vector4 line0, Vector4 line1, out float distance)
    {
        return ClosestPointOnLine<Vector4>(point, line0, line1, out distance);
    }
}
