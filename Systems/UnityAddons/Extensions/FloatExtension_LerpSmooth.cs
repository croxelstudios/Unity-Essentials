using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloatExtension_LerpSmooth
{
    public static float LerpSmooth(this float original, float target, float decay, float deltaTime)
    {
        return Mathf.Lerp(original, target, 1f - Mathf.Exp(-decay * deltaTime));
    }

    public static float LerpAngleSmooth(this float original, float target, float decay, float deltaTime)
    {
        return Mathf.LerpAngle(original, target, 1f - Mathf.Exp(-decay * deltaTime));
    }
}
