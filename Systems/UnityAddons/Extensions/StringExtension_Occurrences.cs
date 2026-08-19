using UnityEngine;

public static class StringExtension_Occurrences
{
    public static int Occurrences(this string source, string substring, bool includeOverlaps = false)
    {
        int count = 0;
        int index = 0;

        while ((index = source.IndexOf(substring, index)) != -1)
        {
            count++;
            index += includeOverlaps ? 1 : substring.Length;
        }

        return count;
    }
}
