using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class IntExtension_RandomExcept
{
    public static int RandomExcept(this int max, IEnumerable<int> exceptions)
    {
        int r = Random.Range(0, max - exceptions.Count());

        //Handle exclusions
        while (exceptions.Contains(r))
        {
            if (r >= max)
            {
                r = -1;
                break;
            }
            r++;
        }
        return r;
    }
}
