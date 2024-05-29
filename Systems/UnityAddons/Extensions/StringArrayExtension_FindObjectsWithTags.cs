using System.Collections.Generic;
using UnityEngine;

public static class StringArrayExtension_FindObjectsWithTags
{
    public static GameObject[] FindObjectsWithTags(this string[] tags, params string[] extraTags)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (string tag in tags)
            list.AddRange(GameObject.FindGameObjectsWithTag(tag));
        foreach (string tag in extraTags)
            list.AddRange(GameObject.FindGameObjectsWithTag(tag));
        return list.ToArray();
    }
}
