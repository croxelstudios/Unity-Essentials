using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloatExtension_Bool
{
    public static bool Bool(this float v)
    {
        return v > 0.5f;
    }
}
