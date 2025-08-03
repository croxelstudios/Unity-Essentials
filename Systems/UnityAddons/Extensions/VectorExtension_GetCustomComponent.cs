using UnityEngine;

public static class VectorExtension_GetCustomComponent
{
    public static float GetCustomComponent(this Vector2 vector, Vector2 axis)
    {
        return Vector2.Dot(vector, axis.normalized);
    }

    public static float GetCustomComponent(this Vector3 vector, Vector3 axis)
    {
        return Vector3.Dot(vector, axis.normalized);
    }
}
