using UnityEngine;
using Random = UnityEngine.Random;

public static class VectorExtension_GetRandom
{
    public static float GetRandom(this float f)
    {
        return Random.Range(-f, f);
    }

    public static Vector2 GetRandom(this Vector2 v)
    {
        return new Vector2(v.x.GetRandom(), v.y.GetRandom());
    }

    public static Vector3 GetRandom(this Vector3 v)
    {
        return new Vector3(v.x.GetRandom(), v.y.GetRandom(), v.z.GetRandom());
    }

    public static Vector4 GetRandom(this Vector4 v)
    {
        return new Vector4(v.x.GetRandom(), v.y.GetRandom(), v.z.GetRandom(), v.w.GetRandom());
    }
}
