using UnityEngine;
using System.Collections.Generic;

public static class StringExtension_ToDisplayName
{
    public static string ToDisplayName(this string name)
    {
        if (name.Length <= 1)
            return name;

        List<int> insertPos = new List<int>();
        string lower = name.ToLower();
        for (int i = 1; i < name.Length; i++)
            if (name[i] != lower[i])
                insertPos.Add(i);

        for (int i = insertPos.Count - 1; i >= 0; i--)
            if (name[insertPos[i] - 1] != ' ')
                name = name.Insert(insertPos[i], " ");

        return name;
    }
}
