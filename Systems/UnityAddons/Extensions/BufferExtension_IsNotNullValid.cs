using UnityEngine;

public static class BufferExtension_IsNotNullValid
{
    public static bool IsNotNullValid(this ComputeBuffer buffer)
    {
        return (buffer != null) && buffer.IsValid();
    }

    public static bool IsNotNullValid(this GraphicsBuffer buffer)
    {
        return (buffer != null) && buffer.IsValid();
    }
}
