using System;

public static class ArrayExtension_Remove
{
    public static T[] Remove<T>(this T[] array, T element)
    {
        if (array.IsNullOrEmpty()) return array;

        int index = Array.IndexOf(array, element);
        if (index < 0) return array;

        T[] newArray = new T[array.Length - 1];
        if (index > 0)
            Array.Copy(array, 0, newArray, 0, index);
        if (index < array.Length - 1)
            Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
        return newArray;
    }

    public static T[] RemoveAt<T>(this T[] array, int index)
    {
        if (array.IsNullOrEmpty()) return array;
        if (!index.IsBetween(0, array.Length)) return array;

        T[] newArray = new T[array.Length - 1];
        int n = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (i != index)
            {
                newArray[n] = array[i];
                n++;
            }
        }
        return newArray;
    }
}
