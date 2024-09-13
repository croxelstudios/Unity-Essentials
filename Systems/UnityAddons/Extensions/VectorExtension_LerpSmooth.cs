using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtension_LerpSmooth
{
    public static Vector4 LerpSmooth(this Vector4 original, Vector4 target, float decay, float deltaTime)
    {
        return Vector4.Lerp(original, target, 1 - Mathf.Exp(-decay * deltaTime));
    }

    public static Vector3 LerpSmooth(this Vector3 original, Vector3 target, float decay, float deltaTime)
    {
        return Vector3.Lerp(original, target, 1 - Mathf.Exp(-decay * deltaTime));
    }

    public static Vector2 LerpSmooth(this Vector2 original, Vector2 target, float decay, float deltaTime)
    {
        return Vector2.Lerp(original, target, 1 - Mathf.Exp(-decay * deltaTime));
    }

    public static Vector3 SLerpSmooth(this Vector3 original, Vector3 target, float decay, float deltaTime)
    {
        return Vector3.Slerp(original, target, 1 - Mathf.Exp(-decay * deltaTime));
    }
}
