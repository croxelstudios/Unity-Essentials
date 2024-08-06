using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IntExtension_IsEven
{
    public static bool IsEven(this int value)
    {
        return (value % 2) == 0;
    }

    public static bool IsEven(this float value)
    {
        return (value % 2f) == 0f;
    }
}
