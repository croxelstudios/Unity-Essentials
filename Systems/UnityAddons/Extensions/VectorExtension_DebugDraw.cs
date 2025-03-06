using UnityEngine;

public static class VectorExtension_DebugDraw
{
    public static void DebugDraw(this Vector3 position, float loopPerc = 0)
    {
        DebugDraw(position, Color.red, loopPerc);
    }

    public static void DebugDraw(this Vector3 position, Color color, float loopPerc = 0)
    {
        Debug.DrawRay(position,
            Quaternion.AngleAxis(loopPerc * 360f, Vector3.up) * Vector3.right,
            color);
    }

    public static void DebugDraw(this Vector3 position, float duration, float loopPerc = 0)
    {
        DebugDraw(position, Color.red, duration, loopPerc);
    }

    public static void DebugDraw(this Vector3 position, Color color, float duration, float loopPerc = 0)
    {
        Debug.DrawRay(position,
            Quaternion.AngleAxis(loopPerc * 360f, Vector3.up) * Vector3.right,
            color, duration);
    }

    public static void DebugDraw(this Vector2 position, float loopPerc = 0)
    {
        DebugDraw(position, Color.red, loopPerc);
    }

    public static void DebugDraw(this Vector2 position, Color color, float loopPerc = 0)
    {
        Debug.DrawRay(position,
            Quaternion.AngleAxis(loopPerc * 360f, Vector3.up) * Vector3.right,
            color);
    }

    public static void DebugDraw(this Vector2 position, float duration, float loopPerc = 0)
    {
        DebugDraw(position, Color.red, duration, loopPerc);
    }

    public static void DebugDraw(this Vector2 position, Color color, float duration, float loopPerc = 0)
    {
        Debug.DrawRay(position,
            Quaternion.AngleAxis(loopPerc * 360f, Vector3.up) * Vector3.right,
            color, duration);
    }
}
