using UnityEngine;
using System.Collections.Generic;

public static class ComponentExtension_GetTransform
{
    public static Transform GetTransform(this Component comp)
    {
        Transform tr = comp as Transform;
        if (tr != null) return tr;
        else
        {
            Behaviour behaviour = comp as Behaviour;
            if (behaviour != null)
                return behaviour.transform;
            else return null;
        }
    }

    public static Transform GetTransform(this Object obj)
    {
        GameObject go = obj as GameObject;
        if (go != null) return go.transform;
        else
        {
            Component comp = obj as Component;
            return comp.GetTransform();
        }
    }

    public static T GetTransforms<T>(this IEnumerable<Component> comp) where T : IEnumerable<Transform>
    {
        return GetTransforms(comp).ToCollection<T, Transform>();
    }

    public static IEnumerable<Transform> GetTransforms(this IEnumerable<Component> comp)
    {
        foreach (Component c in comp)
            yield return c.GetTransform();
    }

    public static T GetTransforms<T>(this IEnumerable<Object> objects) where T : IEnumerable<Object>
    {
        return GetTransforms(objects).ToCollection<T, Object>();
    }

    public static IEnumerable<Transform> GetTransforms(this IEnumerable<Object> objects)
    {
        foreach (Object o in objects)
            yield return o.GetTransform();
    }
}
