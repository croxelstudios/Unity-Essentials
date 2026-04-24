using System.Linq;

public static class ArrayExtension_Remove
{
    public static T[] Remove<T>(this T[] array, T element)
    {
        if (array == null) return null;
        return array.Where(val => !val.Equals(element)).ToArray();
    }

    public static T[] RemoveAt<T>(this T[] arr, int index)
    {
        T[] newArray = new T[arr.Length - 1];
        int n = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            if (i != index)
            {
                newArray[n] = arr[i];
                n++;
            }
        }
        return newArray;
    }
}
