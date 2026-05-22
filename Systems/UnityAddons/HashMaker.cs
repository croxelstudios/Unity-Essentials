using System.Collections;
using System.Linq;
using UnityEngine;

public static class HashMaker
{
    public static int Elements(params object[] objects)
    {
        int hash = 0;
        for (int i = 0; i < objects.Length; i++)
        {
            hash *= 31;
            hash += objects[i].GetDXHash();
        }
        return hash;
    }

    public static int GetDXHash(this object obj)
    {
        if (obj == null)
            return 0;

        if (obj is string)
            return obj.GetHashCode();

        if (obj is IEnumerable enumerable)
            return Elements(enumerable.Cast<object>().ToArray());

        return obj.GetHashCode();
    }
}
