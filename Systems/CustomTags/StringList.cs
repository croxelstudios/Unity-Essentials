using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;

[CreateAssetMenu(menuName = "Croxel Scriptables/String List")]
public class StringList : ScriptableObject
{
    public string[] tags = null;

    public virtual void AddTag(ref string newCustomTag)
    {
        if (!string.IsNullOrWhiteSpace(newCustomTag) && !string.IsNullOrEmpty(newCustomTag))
        {
            tags = tags.Concat(new[] { newCustomTag }).ToArray();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            newCustomTag = null;
        }

    }

    public bool TagExists(string tag)
    {
        return tags.Contains(tag);
    }
}
