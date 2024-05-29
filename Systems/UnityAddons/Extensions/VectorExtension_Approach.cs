using UnityEngine;

public static class VectorExtension_Approach
{
    public static Vector3 Approach(this Vector3 current, Vector3 target, float speed)
    {
        Vector3 dif = target - current;
        return (dif.magnitude < speed) ? target : current + (dif.normalized * speed);
    }

    public static Vector2 Approach(this Vector2 current, Vector2 target, float speed)
    {
        Vector2 dif = target - current;
        return (dif.magnitude < speed) ? target : current + (dif.normalized * speed);
    }
}
