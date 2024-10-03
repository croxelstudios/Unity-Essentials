using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public static class DecimalExtension_ToIntSequence
{
    public static decimal ToDecimalHash(this List<int> list, int digits)
    {
        decimal result = 0m;
        decimal digitizer = 10m.MPow(digits);
        for (int i = 0; i < list.Count; i++)
            result += digitizer.MPow(i) * (list[i] + 1);
        return result;
    }

    public static decimal ToDecimalHash(this int[] array, int digits)
    {
        decimal result = 0m;
        decimal digitizer = 10m.MPow(digits);
        for (int i = 0; i < array.Length; i++)
            result += digitizer.MPow(i) * (array[i] + 1);
        return result;
    }

    public static List<int> ToIntList(this decimal source, int digits)
    {
        int i = 0;
        decimal digitizer = 10m.MPow(digits);
        List<int> result = new List<int>();
        while (source > 0)
        {
            decimal trunk = decimal.Truncate(source / digitizer);
            result.Add((int)(source - (trunk * digitizer) - 1));
            source = trunk;
            i++;
        }
        return result;
    }

    public static int[] ToIntArray(this decimal source, int digits)
    {
        return source.ToIntList(digits).ToArray();
    }

    public static int ToSequenceCount(this decimal source, int digits)
    {
        int i = 0;
        decimal digitizer = 10m.MPow(digits);
        while (source > 0)
        {
            source = decimal.Truncate(source / digitizer);
            i++;
        }
        return i;
    }
}
