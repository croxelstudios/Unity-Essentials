using UnityEngine;

public static class FloatExtension_OffsetedRepeat
{
    public static float OffsetedRepeat(this float value, float lenght, float offset)
    {
        return Mathf.Repeat(value - offset, lenght) + offset;
    }
}
