using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtension_AddComponentCopy
{
    public static T AddComponentCopy<T>(this GameObject go, T toAdd, bool copyBaseValues = false) where T : Component
    {
        Type type = toAdd.GetType();
        Component comp = go.AddComponent(type);
        return comp.GetCopyOf(toAdd, copyBaseValues);
    }

    public static T GetCopyOf<T>(this Component comp, T other, bool copyBaseValues = false) where T : Component
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

        foreach (var pinfo in pinfos)
            if (pinfo.CanWrite)
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } //TO DO: DONT

        foreach (var finfo in finfos)
            finfo.SetValue(comp, finfo.GetValue(other));

        return comp as T;
    }
}
