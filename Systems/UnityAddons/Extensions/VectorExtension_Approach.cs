using UnityEngine;

public static class VectorExtension_Approach
{
    public static Vector4 Approach(this Vector4 current, Vector4 target, float speed, float deltaTime)
    {
        Vector4 dif = target - current;
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

    public static Vector2 Approach(this Vector2 current, Vector2 target, float speed, float deltaTime)
    {
        Vector2 dif = target - current;
        float frameSpeed = speed * deltaTime;
        if (frameSpeed >= dif.magnitude)
            return target;
        else return current + (dif.normalized * frameSpeed);
    }
}
