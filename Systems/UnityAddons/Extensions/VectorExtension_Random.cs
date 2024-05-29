using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class VectorExtension_Random
{
    public static Vector3 GetRandom(this Vector3 v)
    {
        return new Vector3(Random.Range(-v.x, v.x), Random.Range(-v.y, v.y), Random.Range(-v.z, v.z));
    }

    public static Vector2 GetRandom(this Vector2 v)
    {
        return new Vector2(Random.Range(-v.x, v.x), Random.Range(-v.y, v.y));
    }
}
