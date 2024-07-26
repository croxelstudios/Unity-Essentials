using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtension_ChangeLength
{
    public static T[] ChangeLength<T>(this T[] array, int newLength)
    {
        T[] newArray = new T[newLength];
        for (int i = 0; i < newLength; i++)
        {
            if (i < array.Length)
                newArray[i] = array[i];
        }
        return newArray;
    }

    public static T[] ChangeLength<T>(this T[] array, int newLength, T def)
    {
        T[] newArray = new T[newLength];
        for (int i = 0; i < newLength; i++)
        {
            if (i < array.Length)
                newArray[i] = array[i];
            else newArray[i] = def;
        }
        return newArray;
    }
}
