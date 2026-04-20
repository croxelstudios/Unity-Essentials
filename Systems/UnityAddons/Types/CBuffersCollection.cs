using System.Collections.Generic;
using UnityEngine;

public struct CBuffersCollection
{
    Dictionary<string, ComputeBuffer> buffers;

    public ComputeBuffer Get(string key)
    {
        if (buffers.SmartGetValue(key, out ComputeBuffer buffer))
            return buffer;
        else return null;
    }

    public void Set(string key, ComputeBuffer buffer)
    {
        if (buffers.SmartGetValue(key, out ComputeBuffer old))
            old.Dispose();
        buffers = buffers.CreateAdd(key, buffer);
    }

    public bool Has(string key)
    {
        return buffers.NotNullContainsKey(key);
    }

    public bool IsValid(string key)
    {
        if (buffers.SmartGetValue(key, out ComputeBuffer buffer))
            return buffer.IsNotNullValid();
        else return false;
    }

    public bool IsValid(string key, int count)
    {
        if (buffers.SmartGetValue(key, out ComputeBuffer buffer))
            return buffer.IsNotNullValid() && (buffer.count == count);
        else return false;
    }

    public void Release(params string[] keys)
    {
        foreach (string key in keys)
            if (buffers.SmartGetValue(key, out ComputeBuffer buffer))
            {
                buffer.Dispose();
                buffers.Remove(key);
            }
    }

    public void Release()
    {
        if (!buffers.IsNullOrEmpty())
        {
            foreach (ComputeBuffer buffer in buffers.Values)
                buffer.Dispose();
            buffers.Clear();
        }
    }
}

public static class CBuffersCollectionExtensions
{
    public static CBuffersCollection[] Resize(this CBuffersCollection[] array, int newLength)
    {
        CBuffersCollection[] newArray = new CBuffersCollection[newLength];

        if (array == null)
            return newArray;

        for (int i = 0; i < newLength; i++)
        {
            if (i < array.Length)
                newArray[i] = array[i];
        }

        for (int i = newLength; i < array.Length; i++)
            array[i].Release();

        return newArray;
    }

    public static IList<CBuffersCollection> TryRelease(this IList<CBuffersCollection> bufferCollection)
    {
        if (!bufferCollection.IsNullOrEmpty())
            for (int i = 0; i < bufferCollection.Count; i++)
                bufferCollection[i].Release();
        return null;
    }
}
