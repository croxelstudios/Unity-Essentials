using System.Collections.Generic;

public static class ListExtension_ReorderElement
{
    public static void ReorderElement<T>(this List<T> list, int originalIndex, int targetIndex)
    {
        list.Insert(targetIndex, list[originalIndex]);
        if (targetIndex < originalIndex) originalIndex++;
        list.RemoveAt(originalIndex);
        list.TrimExcess();
    }
}
