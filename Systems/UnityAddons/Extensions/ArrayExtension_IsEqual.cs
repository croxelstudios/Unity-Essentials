using System;
using System.Collections.Generic;
using System.Linq;

public static class ArrayExtension_IsEqual
{
    public static bool IsEqual<T>(this IEnumerable<T> a, IEnumerable<T> b) where T : IEquatable<T>
    {
        if (a == null || b == null)
            return (a == null) && (b == null);

        if (a.Count() != b.Count())
            return false;

        for (int i = 0; i < a.Count(); i++)
            if (!a.ElementAt(i).Equals(b.ElementAt(i)))
                return false;

        return true;
    }

    public static int ElementsHash<T>(this IEnumerable<T> array) where T : IEquatable<T>
    {
        return HashMaker.Elements(array.Cast<object>().ToArray());
    }
}
