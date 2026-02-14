using UnityEngine;

public static class ComputeBuffer_IsNotNullValid
{
    public static bool IsNotNullValid(this ComputeBuffer buffer)
    {
        return (buffer != null) && buffer.IsValid();
    }
}
