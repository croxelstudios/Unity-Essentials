using System.Collections.Generic;
using System.Linq;

public static class ArrayExtension_IsNullOrEmpty
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
    {
        return (list == null) || (!list.Any());
    }

    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }
}
