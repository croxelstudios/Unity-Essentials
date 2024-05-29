using UnityEngine;

public static class VectorExtension_GetCustomComponent
{
    //TO DO: I have to test if this is faster than Vector3.Project().magnitude
    public static float GetCustomComponent(this Vector2 vector, Vector2 axis)
    {
        return Mathf.Cos(Vector2.Angle(axis, vector) * Mathf.Deg2Rad)
        * vector.magnitude;
    }

    public static float GetCustomComponent(this Vector3 vector, Vector3 axis)
    {
        return Mathf.Cos(Vector3.Angle(axis, vector) * Mathf.Deg2Rad)
        * vector.magnitude;
    }
}
