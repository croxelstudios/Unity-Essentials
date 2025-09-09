using UnityEngine;

public static class FloatExtension_Approach
{
    public static float Approach(this float current, float target, float speed, float deltaTime)
    {
        float dif = target - current;
        float frameSpeed = speed * deltaTime;
        if (frameSpeed >= Mathf.Abs(dif))
            return target;
        else return current + (Mathf.Sign(dif) * frameSpeed);
    }

    public static Vector2 Approach(this Vector2 current, Vector2 target, float speed, float deltaTime)
    {
        Vector2 dif = target - current;
        float frameSpeed = speed * deltaTime;
        if (frameSpeed >= dif.magnitude)
            return target;
        else return current + (dif.normalized * frameSpeed);
    }

    public static Vector3 Approach(this Vector3 current, Vector3 target, float speed, float deltaTime)
    {
        Vector3 dif = target - current;
        float frameSpeed = speed * deltaTime;
        if (frameSpeed >= dif.magnitude)
            return target;
        else return current + (dif.normalized * frameSpeed);
    }

    public static Vector4 Approach(this Vector4 current, Vector4 target, float speed, float deltaTime)
    {
        Vector4 dif = target - current;
        float frameSpeed = speed * deltaTime;
        if (frameSpeed >= dif.magnitude)
            return target;
        else return current + (dif.normalized * frameSpeed);
    }

    public static Color Approach(this Color current, Color target, float speed, float deltaTime)
    {
        Color dif = target - current;
        float frameSpeed = speed * deltaTime;
        if (frameSpeed >= ((Vector4)dif).magnitude)
            return target;
        else return current + (Color)(((Vector4)dif).normalized * frameSpeed);
    }

    public static Quaternion Approach(this Quaternion current, Quaternion target, float speed, float deltaTime)
    {
        Quaternion dif = target.Subtract(current);
        float frameSpeed = speed * deltaTime;
        dif.ToAngleAxis(out float angle, out Vector3 axis);
        if (frameSpeed >= angle)
            return target;
        else return current.Add(Quaternion.AngleAxis(frameSpeed, axis));
    }
}
