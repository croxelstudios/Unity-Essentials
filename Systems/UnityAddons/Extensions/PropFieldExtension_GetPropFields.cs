using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

public static class PropFieldExtension_GetPropFields
//TO DO: Maybe create a whole new "PropFieldInfo" class inheriting from MemberInfo
//and having implicit conversions to the other types
{
    public static MemberInfo[] GetPropFields(this Type type,
        bool copyBaseValues = true)
    {
        BindingFlags flags =
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.DeclaredOnly;

        List<MemberInfo> infos = new List<MemberInfo>();
        if (copyBaseValues)
        {
            Type wType = type;
            do
            {
                infos.AddRange(wType.GetPropFields(flags));
                wType = wType?.BaseType;
            }
            while ((wType != null) && (wType != typeof(Component)) &&
                (wType != typeof(MonoBehaviour)));
        }
        else infos.AddRange(type.GetPropFields(flags));
        return infos.ToArray();
    }

    public static MemberInfo[] GetPropFields(this Type type,
        BindingFlags bindingAttr = BindingFlags.Default)
    {
        List<MemberInfo> infos = new List<MemberInfo>();
        infos.AddRange(type.GetProperties(bindingAttr));
        infos.AddRange(type.GetFields(bindingAttr));
        return infos.ToArray();
    }

    public static bool CanWrite(this MemberInfo info)
    {
        PropertyInfo prop = info as PropertyInfo;
        if (prop != null)
            return prop.CanWrite;
        FieldInfo field = info as FieldInfo;
        if (field != null)
            return !field.IsInitOnly;
        throw new ArgumentException(
            "MemberInfo must be of type PropertyInfo or FieldInfo", nameof(info));
    }

    public static Expression PropField(this Expression instance, MemberInfo memberInfo)
    {
        PropertyInfo prop = memberInfo as PropertyInfo;
        if (prop != null)
            return Expression.Property(instance, prop);
        FieldInfo field = memberInfo as FieldInfo;
        if (field != null)
            return Expression.Field(instance, field);
        throw new ArgumentException(
            "MemberInfo must be of type PropertyInfo or FieldInfo", nameof(memberInfo));
    }

    public static Type PropFieldType(this MemberInfo info)
    {
        PropertyInfo prop = info as PropertyInfo;
        if (prop != null)
            return prop.PropertyType;
        FieldInfo field = info as FieldInfo;
        if (field != null)
            return field.FieldType;
        throw new ArgumentException(
            "MemberInfo must be of type PropertyInfo or FieldInfo", nameof(info));
    }

    public static object GetPropFieldValue(this MemberInfo info, object obj)
    {
        PropertyInfo prop = info as PropertyInfo;
        if (prop != null)
            return prop.GetValue(obj);
        FieldInfo field = info as FieldInfo;
        if (field != null)
            return field.GetValue(obj);
        throw new ArgumentException(
            "MemberInfo must be of type PropertyInfo or FieldInfo", nameof(info));
    }

    public static void SetPropFieldValue(this MemberInfo info, object obj, object value)
    {
        PropertyInfo prop = info as PropertyInfo;
        if (prop != null)
        {
            prop.SetValue(obj, value);
            return;
        }
        FieldInfo field = info as FieldInfo;
        if (field != null)
        {
            field.SetValue(obj, value);
            return;
        }
        throw new ArgumentException(
            "MemberInfo must be of type PropertyInfo or FieldInfo", nameof(info));
    }

    public static void CopyPropFieldValue(this MemberInfo info, object target, object source)
    {
        info.SetPropFieldValue(target, info.GetPropFieldValue(source));
    }
}
