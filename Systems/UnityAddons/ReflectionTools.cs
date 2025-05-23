using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ReflectionTools
{
    public static T GetValue<T>(object inObj, string fieldPath)
    {
        object obj = GetFieldValue(inObj, fieldPath);
        if (obj == null) return default(T);
        return (T)obj;
    }

    public static object GetFieldValue(object inObj, string fieldPath)
    {
        fieldPath = fieldPath.Replace(".Array.data", "");
        string[] fieldStructure = fieldPath.Split('.');
        for (int i = 0; i < fieldStructure.Length; i++)
        {
            string text = fieldStructure[i].BreakDownArrayVariable(out int index);
            if (index >= 0) inObj = GetFieldValueWithIndex(text, inObj, index);
            else inObj = GetFieldValue(text, inObj);
        }
        return inObj;
    }

    public static bool SetValue<T>(object inObj, string fieldPath, T newValue)
    {
        return SetFieldValue(inObj, fieldPath, newValue);
    }

    public static bool SetFieldValue(object inObj, string fieldPath, object newValue)
    {
        fieldPath = fieldPath.Replace(".Array.data", "");
        string[] fieldStructure = fieldPath.Split('.');
        for (int i = 0; i < fieldStructure.Length - 1; i++)
        {
            string txt = fieldStructure[i].BreakDownArrayVariable(out int ind);
            if (ind >= 0) inObj = GetFieldValueWithIndex(txt, inObj, ind);
            else inObj = GetFieldValue(txt, inObj);
        }

        string fieldName = fieldStructure.Last();

        string text = fieldName.BreakDownArrayVariable(out int index);
        if (index >= 0) return SetFieldValueWithIndex(text, inObj, index, newValue);
        else return SetFieldValue(text, inObj, newValue);
    }

    public static Type GetType(object inObj, string fieldPath)
    {
        fieldPath = fieldPath.Replace(".Array.data", "");
        string[] fieldStructure = fieldPath.Split('.');
        Type type = null;
        for (int i = 0; i < fieldStructure.Length; i++)
        {
            string text = fieldStructure[i].BreakDownArrayVariable(out int index);
            if (index >= 0) type = GetField(text, inObj)?.FieldType?.GetElementType();
            else
            {
                type = GetField(text, inObj)?.FieldType;
                if (type == null) type = GetProperty(text, inObj)?.PropertyType;
            }
        }
        return type;
    }

    private static FieldInfo GetField(string fieldName, object obj,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        Type originType = obj?.GetType();
        Type type = originType;
        FieldInfo field;
        do
        {
            field = type?.GetField(fieldName, bindings);
            type = type?.BaseType;
        }
        while ((field == null) && (type != null));
        if (field != null)
        {
            return field;
        }
        return default;
    }

    private static PropertyInfo GetProperty(string fieldName, object obj,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        Type type = obj.GetType();
        PropertyInfo property;
        do
        {
            property = type.GetProperty(fieldName, bindings);
            type = type.BaseType;
        }
        while ((property == null) && (type != null));
        if (property != null)
        {
            return property;
        }
        return default;
    }

    private static object GetFieldValue(string fieldName, object obj,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        FieldInfo field = GetField(fieldName, obj, bindings);
        if (field != null) return field.GetValue(obj);
        else
        {
            PropertyInfo property = GetProperty(fieldName, obj, bindings);
            if (property != null) return property.GetValue(obj);
        }
        return default;
    }

    private static object GetFieldValueWithIndex(string fieldName, object obj, int index,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        FieldInfo field = GetField(fieldName, obj, bindings);
        if (field != null)
        {
            object list = field.GetValue(obj);
            //if (list.GetType().IsArray)
            //{
            //    return ((object[])list)[index];
            //}
            //else
            if (list is IEnumerable)
            {
                return ((IList)list)[index];
            }
        }
        else
        {
            PropertyInfo property = GetProperty(fieldName, obj, bindings);
            if (property != null)
            {
                object list = property.GetValue(obj);
                //if (list.GetType().IsArray)
                //{
                //    return ((object[])list)[index];
                //}
                //else
                if (list is IEnumerable)
                {
                    return ((IList)list)[index];
                }
            }
        }
        return default;
    }

    public static bool SetFieldValue(string fieldName, object obj, object value,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        FieldInfo field = GetField(fieldName, obj, bindings);
        if (field != null)
        {
            try { Convert.ChangeType(value, field.FieldType); } //WARNING: So ugly...
            catch { return false; }

            field.SetValue(obj, value);
            return true;
        }
        else
        {
            PropertyInfo property = GetProperty(fieldName, obj, bindings);
            if (property != null)
            {
                try { Convert.ChangeType(value, property.PropertyType); } //WARNING: So ugly...
                catch { return false; }

                property.SetValue(obj, value);
                return true;
            }
        }
        return false;
    }

    public static bool SetFieldValueWithIndex(string fieldName, object obj, int index, object value,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        FieldInfo field = GetField(fieldName, obj, bindings);
        if (field != null)
        {
            object list = field.GetValue(obj);
            //if (list.GetType().IsArray)
            //{
            //    ((object[])list)[index] = value; //Breaks when using float arrays
            //    return true;
            //}
            //else
            if (list is IEnumerable)
            {
                ((IList)list)[index] = value;
                return true;
            }
        }
        else
        {
            PropertyInfo property = GetProperty(fieldName, obj, bindings);
            if (property != null)
            {
                object list = property.GetValue(obj);
                if (list.GetType().IsArray)
                {
                    ((object[])list)[index] = value;
                    return true;
                }
                else if (list is IEnumerable)
                {
                    ((IList)list)[index] = value;
                    return true;
                }
            }
        }
        return false;
    }

#if UNITY_EDITOR
    public static T GetValue<T>(SerializedProperty property)
    {
        return (T)GetFieldValue(property);
    }

    public static object GetFieldValue(SerializedProperty property)
    {
        return GetFieldValue(property.serializedObject.targetObject, property.propertyPath);
    }

    public static bool SetValue<T>(SerializedProperty property, object newValue)
    {
        return SetFieldValue(property, newValue);
    }

    public static bool SetFieldValue(SerializedProperty property, object newValue)
    {
        return SetFieldValue(property.serializedObject.targetObject, property.propertyPath, newValue);
    }
#endif
}
