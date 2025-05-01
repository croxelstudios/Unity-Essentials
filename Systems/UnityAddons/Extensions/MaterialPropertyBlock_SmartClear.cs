using UnityEngine;

public static class MaterialPropertyBlock_SmartClear
{
    public static void SmartClear(this MaterialPropertyBlock mpb)
    {
        if (mpb != null) mpb.Clear();
    }
}
