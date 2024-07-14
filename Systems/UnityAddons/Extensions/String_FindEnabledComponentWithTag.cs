using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class String_FindEnabledComponentWithTag
{
    public static T FindComponentWithTag<T>(this string tag, bool onlyEnabled = true)
        where T : Behaviour
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        T component = null;
        foreach (GameObject obj in objs)
        {
            component = obj.GetComponent<T>();
            if (component != null)
            {
                if ((!onlyEnabled) || component.enabled) break;
                else component = null;
            }
        }
        return component;
    }

    public static T[] FindComponentsWithTag<T>(this string tag, bool onlyEnabled = true)
        where T : Behaviour
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        List<T> components = new List<T>();
        foreach (GameObject obj in objs)
        {
            T component = obj.GetComponent<T>();
            if (component != null)
            {
                if ((!onlyEnabled) || component.enabled)
                    components.Add(component);
            }
        }
        return components.ToArray();
    }
}
