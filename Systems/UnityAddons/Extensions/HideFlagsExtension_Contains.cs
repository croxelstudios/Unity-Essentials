using UnityEngine;

public static class HideFlagsExtension_Contains
{
    public static bool Contains(this HideFlags hideFlags, HideFlags flag)
    {
        return (hideFlags & flag) == flag;
    }
}
