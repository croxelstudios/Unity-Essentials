using UnityEngine;

public static class VectorExtension_DebugDraw
{
    public static void DebugDraw(this Vector3 position, float i = 0)
    {
        DebugDraw(position, Color.red, i);
    }

    public static void DebugDraw(this Vector3 position, Color color, float i = 0)
    {
        Debug.DrawRay(position,
            Quaternion.AngleAxis(i * 360f, Vector3.up) * Vector3.right,
            color);
    }

    public static void DebugDraw(this Vector2 position, float i = 0)
    {
        DebugDraw(position, Color.red, i);
    }

    public static void DebugDraw(this Vector2 position, Color color, float i = 0)
    {
        Debug.DrawRay(position,
            Quaternion.AngleAxis(i * 360f, Vector3.up) * Vector3.right,
            color);
    }
}
