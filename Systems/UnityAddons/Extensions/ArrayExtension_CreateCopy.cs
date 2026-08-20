using UnityEngine;

public static class ArrayExtension_CreateCopy
{
    public static T[] CreateCopy<T>(this T[] array)
    {
        if (array == null)
            return null;
        T[] copy = new T[array.Length];
        for (int i = 0; i < array.Length; i++)
            copy[i] = array[i];
        return copy;
    }
}
