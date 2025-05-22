using UnityEngine;
using System.Collections.Generic;

public static class StringExtension_ToDisplayName
{
    public static string ToDisplayName(this string name)
    {
        if (name.Length <= 1)
            return name;

        //Replace "_" by space
        name = name.Replace("_", " ");
        //

        //Insert Spaces
        List<int> insertPos = new List<int>();
        string lower = name.ToLower();
        for (int i = 1; i < name.Length; i++)
            if ((name[i] != lower[i]) ||
                (char.IsLetter(name[i]) != char.IsLetter(name[i - 1])))
                insertPos.Add(i);

        for (int i = insertPos.Count - 1; i >= 0; i--)
            if (name[insertPos[i] - 1] != ' ')
                name = name.Insert(insertPos[i], " ");
        //

        //First letter is upper case
        name = name.Insert(0, new string(char.ToUpper(name[0]), 1)).Remove(1, 1);
        //

        //All post-space letters are uppercase
        for (int i = 1; i < name.Length; i++)
            if ((char.IsLetter(name[i]) != char.IsLetter(name[i - 1])))
                name = name.Insert(i, new string(char.ToUpper(name[i]), 1)).Remove(i + 1, 1);
        //

        //Remove spaces before first letter
        int remove = 0;
        for (int i = 0; i < name.Length; i++)
            if (name[i] != ' ')
                break;
            else remove++;
        name = name.Remove(0, remove);
        //

        return name;
    }
}
