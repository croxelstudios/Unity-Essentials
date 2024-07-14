using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloatExtension_BezierCurve
{
    public static float BezierCurve(this float t)
    {
        return t * t * (3f - (2f * t));
    }
}
