using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

public static class StringExtension_FindComponentWithTag
{
    public static T FindComponentWithTag<T>(this string tag) where T : Component
    {
        Object[] objs = GameObject.FindObjectsByType(typeof(T), FindObjectsSortMode.None);
        for (int i = 0; i < objs.Length; i++)
        {
            if (((Component)objs[i]).tag == tag)
                return (T)objs[i];
        }
        return null;
    }
}
