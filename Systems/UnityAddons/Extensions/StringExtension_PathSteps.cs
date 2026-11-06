using UnityEngine;

public static class StringExtension_PathSteps
{
    public static string[] PathSteps(this string path)
    {
        path = path.Replace(".Array.data[", "[");
        return path.Split('.');
    }
}
