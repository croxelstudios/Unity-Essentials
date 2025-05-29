using System.Collections.Generic;
using UnityEngine;

public static class StringArrayExtension_ToGUIContent
{
    public static T ToGUIContent<T>(this IEnumerable<string> names) where T : IEnumerable<GUIContent>
    {
        return ToGUIContent(names).ToCollection<T, GUIContent>();
    }

    public static IEnumerable<GUIContent> ToGUIContent(this IEnumerable<string> names)
    {
        foreach (string name in names)
            yield return new GUIContent(name);
    }
}
