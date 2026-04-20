using UnityEngine;

public static class ArrayExtension_Resize
{
    public static ComputeBuffer[] Resize(this ComputeBuffer[] array, int newLength)
    {
        ComputeBuffer[] newArray = new ComputeBuffer[newLength];

        if (array == null)
            return newArray;

        for (int i = 0; i < newLength; i++)
        {
            if (i < array.Length)
                newArray[i] = array[i];
        }

        for (int i = newLength; i < array.Length; i++)
            array[i].Dispose();

        return newArray;
    }

    public static GraphicsBuffer[] Resize(this GraphicsBuffer[] array, int newLength)
    {
        GraphicsBuffer[] newArray = new GraphicsBuffer[newLength];

        if (array == null)
            return newArray;

        for (int i = 0; i < newLength; i++)
        {
            if (i < array.Length)
                newArray[i] = array[i];
        }

        for (int i = newLength; i < array.Length; i++)
            array[i].Dispose();

        return newArray;
    }

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
