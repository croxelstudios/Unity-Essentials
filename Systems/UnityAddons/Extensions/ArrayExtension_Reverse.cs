using UnityEngine;

public static class ArrayExtension_Reverse
{
    public static T[] Reverse<T>(this T[] input)
    {
        T[] reversed = new T[input.Length];
        for (int i = 0; i < input.Length; i++)
            reversed[i] = input[^(i + 1)];
        return reversed;
    }
}
