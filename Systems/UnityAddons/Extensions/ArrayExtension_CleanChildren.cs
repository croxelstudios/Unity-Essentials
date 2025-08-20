using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ArrayExtension_CleanChildren
{
    public static IEnumerable<T> CleanChildren<T>(this IEnumerable<T> objs) where T : Object
    {
        List<T> list = new List<T>();
        list.AddRange(objs);
        foreach (T objp in objs)
            foreach (T obj in objs)
                if ((obj != objp) && obj.GetTransform().IsChildOf(objp.GetTransform()))
                    list.Remove(obj);
        return list;
    }

    public static T[] CleanChildren<T>(this T[] objs) where T : Object
    {
        return CleanChildren((IEnumerable<T>)objs).ToArray();
    }

    public static List<T> CleanChildren<T>(this List<T> objs) where T : Object
    {
        return CleanChildren((IEnumerable<T>)objs).ToList();
    }

    public static HashSet<T> CleanChildren<T>(this HashSet<T> objs) where T : Object
    {
        return CleanChildren((IEnumerable<T>)objs).ToHashSet();
    }
}
