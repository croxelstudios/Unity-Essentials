using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class ArrayExtension_IsNullOrEmpty
{
    public static bool IsNullOrEmpty<T>(this T[] array)
    {
        return (array == null) || (array.Length == 0);
    }

    public static bool IsNullOrEmpty<T>(this IList<T> list)
    {
        return (list == null) || (list.Count == 0);
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
    {
        return (list == null) || (list.Count() == 0);
    }
}
