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
}
