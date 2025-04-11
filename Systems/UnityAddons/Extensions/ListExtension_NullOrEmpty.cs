using System.Collections.Generic;
using UnityEngine;

public static class ListExtension_NullOrEmpty
{
    public static bool NullOrEmpty<T>(this IList<T> list)
    {
        if (list != null)
        {
            return list.Count == 0;
        }

        return true;
    }
}
