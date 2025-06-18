public static class ArrayExtension_Resize
{
    public static T[] Resize<T>(this T[] array, int newLength)
    {
        T[] newArray = new T[newLength];

        if (array == null)
            return newArray;

        for (int i = 0; i < newLength; i++)
        {
            if (i < array.Length)
                newArray[i] = array[i];
        }
        return newArray;
    }

    public static T[] Resize<T>(this T[] array, int newLength, T def)
    {
        T[] newArray = new T[newLength];

        if (array == null)
            array = new T[0];

        for (int i = 0; i < newLength; i++)
        {
            if (i < array.Length)
                newArray[i] = array[i];
            else newArray[i] = def;
        }
        return newArray;
    }
}
