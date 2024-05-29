using UnityEngine;
using UnityEngine.Rendering;

public static class StaticRTHandler
{
    public static bool hasBeeninitialized = false;

    public static void Init()
    {
        if (!hasBeeninitialized) RTHandles.Initialize(Screen.width, Screen.height);
    }
}
