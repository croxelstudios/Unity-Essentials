using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BufferCollectionExtension_TryRelease
{
    public static IList<ComputeBuffer> TryRelease(this IList<ComputeBuffer> bufferCollection)
    {
        if (!bufferCollection.IsNullOrEmpty())
            for (int i = 0; i < bufferCollection.Count; i++)
                bufferCollection[i] = bufferCollection[i].ReleaseToNull();
        return null;
    }

    public static IList<GraphicsBuffer> TryRelease(this IList<GraphicsBuffer> bufferCollection)
    {
        if (!bufferCollection.IsNullOrEmpty())
            for (int i = 0; i < bufferCollection.Count; i++)
                bufferCollection[i] = bufferCollection[i].ReleaseToNull();
        return null;
    }
}
