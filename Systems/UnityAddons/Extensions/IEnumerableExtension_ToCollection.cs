using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class IEnumerableExtension_ToCollection
{
    public static C ToCollection<C, T>(this IEnumerable<T> elements) where C : IEnumerable<T>
    {
        if (typeof(C) == typeof(T[]))
            elements = elements.ToArray();
        else if (typeof(C) == typeof(List<T>))
            elements = elements.ToList();
        else if (typeof(T) == typeof(HashSet<T>))
            elements = elements.ToHashSet();
        else
            Debug.LogError($"{typeof(C)} is unsupported");
        return (C)elements;
    }
}
