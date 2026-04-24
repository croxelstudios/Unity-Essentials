using UnityEngine;
using System;
using System.Collections.Generic;

public static class GameObjectExtension_GetEnabledComponents
{
    public static Component[] GetEnabledComponents(this GameObject gameObject, Type type)
    {
        List<Component> list = new List<Component>();
        list.AddRange(gameObject.GetComponents(type));
        for (int i = (list.Count - 1); i >= 0; i--)
        {
            if (list[i] is Behaviour behaviour)
            {
                if (!behaviour.enabled)
                    list.RemoveAt(i);
            }
            else if (list[i] is Renderer renderer)
            {
                if (!renderer.enabled)
                    list.RemoveAt(i);
            }
        }
        return list.ToArray();
    }

    public static Component[] GetEnabledComponents(this Component component, Type type)
    {
        return component.gameObject.GetEnabledComponents(type);
    }

    public static T[] GetEnabledRenderers<T>(this GameObject gameObject) where T : Renderer
    {
        List<T> list = new List<T>();
        list.AddRange(gameObject.GetComponents<T>());
        for (int i = (list.Count - 1); i >= 0; i--)
            if (!list[i].enabled)
                list.RemoveAt(i);
        return list.ToArray();
    }

    public static T[] GetEnabledRenderers<T>(this Component component) where T : Renderer
    {
        return component.gameObject.GetEnabledRenderers<T>();
    }

    public static T[] GetEnabledBehaviours<T>(this GameObject gameObject) where T : Behaviour
    {
        List<T> list = new List<T>();
        list.AddRange(gameObject.GetComponents<T>());
        for (int i = (list.Count - 1); i >= 0; i--)
            if (!list[i].enabled)
                list.RemoveAt(i);
        return list.ToArray();
    }

    public static T[] GetEnabledBehaviours<T>(this Component component) where T : Behaviour
    {
        return component.gameObject.GetEnabledBehaviours<T>();
    }
}
