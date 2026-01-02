using UnityEngine;

public static class ComputeBufferExtension_ReleaseToNull
{
    public static ComputeBuffer ReleaseToNull(this ComputeBuffer buffer)
    {
        buffer?.Release();
        return null;
    }
}
