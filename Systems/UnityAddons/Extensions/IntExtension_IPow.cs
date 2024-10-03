using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IntExtension_IPow
{
    public static int IPow(this int b, int exp)
    {
        int result = 1;
        while (exp > 0)
        {
            if ((exp & 1) != 0)
                result *= b;
            exp >>= 1;
            b *= b;
        }
        return result;
    }

    public static decimal MPow(this decimal b, int exp)
    {
        decimal result = 1;
        while (exp > 0)
        {
            if ((exp & 1) != 0)
                result *= b;
            exp >>= 1;
            b *= b;
        }
        return result;
    }
}
