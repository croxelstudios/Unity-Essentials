using UnityEngine;

public static class FloatExtension_Approach
{
    public static float Approach(this float current, float target, float speed)
    {
        return (target > current) ? Mathf.Min(target, current + speed) : Mathf.Max(target, current - speed);
    }
}
