using System.Collections.Generic;
using System.Linq;

public static class ArrayExtension_LoopOffset
{
    public static T[] LoopOffset<T>(this IEnumerable<T> list, int offset)
    {
        int count = list.Count();
        T[] tmp = new T[count];
        for (int i = 0; i < count; i++)
        {
            int off = (i + offset).Loop(count);
            tmp[i] = list.ElementAt(off);
        }
        return tmp;
    }
}
