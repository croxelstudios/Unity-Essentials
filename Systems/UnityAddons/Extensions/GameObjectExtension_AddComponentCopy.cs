using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.Linq;

public static class GameObjectExtension_AddComponentCopy
{
    public static T AddComponentCopy<T>(this GameObject go, T toAdd, string[] exclusions, bool copyBaseValues = true) where T : Component
    {
        Type type = toAdd.GetType();
        Component comp = go.AddComponent(type);
        return comp.GetCopyOf(toAdd, exclusions, copyBaseValues);
    }

    public static T AddComponentCopy<T>(this GameObject go, T toAdd, bool copyBaseValues = true) where T : Component
    {
        return go.AddComponentCopy(toAdd, null, copyBaseValues);
    }

    public static T GetCopyOf<T>(this Component comp, T other, string[] exclusions, bool copyBaseValues = true) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        
        List<PropertyInfo> pinfos = new List<PropertyInfo>();
        List<FieldInfo> finfos = new List<FieldInfo>();
        if (copyBaseValues)
        {
            Type wType = type;
            do
            {
                pinfos.AddRange(wType.GetProperties(flags));
                finfos.AddRange(wType.GetFields(flags));
                wType = wType?.BaseType;
            }
            while ((wType != null) && (wType != typeof(Component)) && (wType != typeof(MonoBehaviour)));
        }
        else
        {
            pinfos.AddRange(type.GetProperties(flags));
            finfos.AddRange(type.GetFields(flags));
        }

        foreach (PropertyInfo pinfo in pinfos)
            if (pinfo.CanWrite && !CreatesInstance(type, pinfo) &&
                ((exclusions == null) || (!exclusions.Contains(pinfo.Name))))
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } //TO DO: DONT

        foreach (FieldInfo finfo in finfos)
            if ((exclusions == null) || (!exclusions.Contains(finfo.Name)))
                finfo.SetValue(comp, finfo.GetValue(other));

        return comp as T;
    }

    public static T GetCopyOf<T>(this Component comp, T other, bool copyBaseValues = true) where T : Component
    {
        return comp.GetCopyOf(other, null, copyBaseValues);
    }
    
    public static bool IsOrInheritsFrom(this Type type, Type baseType)
    {
        return type == baseType || type.IsSubclassOf(baseType);
    }

    static bool CreatesInstance(Type type, PropertyInfo info)
    {
        return (type.IsOrInheritsFrom(typeof(MeshFilter)) && (info.Name == "mesh")) ||
            (type.IsOrInheritsFrom(typeof(Renderer)) &&
            ((info.Name == "material") || (info.Name == "materials")));
    }
}
