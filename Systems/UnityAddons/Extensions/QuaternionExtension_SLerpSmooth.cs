using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuaternionExtension_SLerpSmooth
{
    public static Quaternion LerpSmooth(this Quaternion original, Quaternion target, float decay, float deltaTime)
    {
        return Quaternion.Lerp(original, target, 1 - Mathf.Exp(-decay * deltaTime));
    }

    public static Quaternion SLerpSmooth(this Quaternion original, Quaternion target, float decay, float deltaTime)
    {
        return Quaternion.Slerp(original, target, 1 - Mathf.Exp(-decay * deltaTime));
    }
}
