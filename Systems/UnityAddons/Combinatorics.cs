using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;

public static class Combinatorics
{
    public static List<T[]> CartesianProduct<T>(T[][] collections, int currentRow)
    {
        List<T[]> res = new List<T[]>();
        if (currentRow < collections.Length)
        {
            List<T[]> subproblem_result =
                CartesianProduct(collections, currentRow + 1);
            int size = subproblem_result.Count;

            for (int i = 0; i < collections[currentRow].Length; ++i)
            {
                if ((subproblem_result != null) && (subproblem_result.Count > 0))
                {
                    for (int j = 0; j < size; ++j)
                    {
                        List<T> current_combs = new List<T>
                        { collections[currentRow][i] };
                        current_combs.AddRange(subproblem_result[j]);
                        res.Add(current_combs.ToArray());
                    }
                }
                else
                {
                    List<T> current_combs = new List<T>
                        { collections[currentRow][i] };
                    res.Add(current_combs.ToArray());
                }
            }
        }

        return res;
    }
}
