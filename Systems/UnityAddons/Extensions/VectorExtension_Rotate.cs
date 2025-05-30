using UnityEngine;

public static class VectorExtension_Rotate
{
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        float tx = v.x;
        float ty = v.y;

        return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
    }

    public static Vector3 Rotate(this Vector3 v, float degrees, Vector3 axis)
    {
        return Quaternion.AngleAxis(degrees, axis) * v;
    }

    //TO DO: 4D?
}
