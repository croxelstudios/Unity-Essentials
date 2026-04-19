using UnityEngine;

public static class BufferExtension_ReleaseToNull
{
    public static ComputeBuffer ReleaseToNull(this ComputeBuffer buffer)
    {
        buffer?.Release();
        return null;
    }

    public static GraphicsBuffer ReleaseToNull(this GraphicsBuffer buffer)
    {
        buffer?.Release();
        return null;
    }
}
