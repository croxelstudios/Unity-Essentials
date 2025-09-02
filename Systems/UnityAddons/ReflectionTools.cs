using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ReflectionTools
{
    static Dictionary<object, Dictionary<string, ObjectInfo>> cachedObjectInfo = new Dictionary<object, Dictionary<string, ObjectInfo>>();
    public struct ObjectInfo
    {
        public FieldInfo fieldInfo;
        public PropertyInfo propInfo;
        public object obj;
        public int index;

        public ObjectInfo(FieldInfo fieldInfo, PropertyInfo propInfo, object obj, int index)
        {
            this.fieldInfo = fieldInfo;
            this.propInfo = propInfo;
            this.obj = obj;
            this.index = index;
        }

        public bool IsNull()
        {
            return (fieldInfo == null) && (propInfo == null);
        }
    }

    public static T GetValue<T>(object inObj, string fieldPath, bool cacheObjectInfo = true)
    {
        object obj = GetFieldValue(inObj, fieldPath, cacheObjectInfo);
        if (obj == null) return default(T);
        return (T)obj;
    }

    public static object GetFieldValue(object inObj, string fieldPath, bool cacheObjectInfo = true)
    {
        /*
        fieldPath = fieldPath.Replace(".Array.data", "");
        string[] fieldStructure = fieldPath.Split('.');
        for (int i = 0; i < fieldStructure.Length; i++)
        {
            string text = fieldStructure[i].BreakDownArrayVariableName(out int index);
            if (index >= 0) inObj = GetFieldValueWithIndex(text, inObj, index);
            else inObj = GetFieldValue(text, inObj);
        }*/
        ObjectInfo objectInfo;
        if (!cacheObjectInfo)
            objectInfo = GetObjectInfo(inObj, fieldPath);
        else
            objectInfo = GetCachedInfo(inObj, fieldPath);
        return GetFieldValue(objectInfo);
    }

    public static bool SetValue<T>(object inObj, string fieldPath, T newValue, bool cacheObjectInfo = true)
    {
        return SetFieldValue(inObj, fieldPath, newValue, cacheObjectInfo);
    }

    public static bool SetFieldValue(object inObj, string fieldPath, object newValue, bool cacheObjectInfo = true)
    {
        ObjectInfo objectInfo;
        if (!cacheObjectInfo)
            objectInfo = GetObjectInfo(inObj, fieldPath);
        else
            objectInfo = GetCachedInfo(inObj, fieldPath);
        return SetFieldValue(objectInfo, newValue);
    }

    static ObjectInfo GetObjectInfo(object inObj, string fieldPath,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        if (cachedObjectInfo.SmartGetValue(inObj, fieldPath, out ObjectInfo result))
            return result;

        fieldPath = fieldPath.Replace(".Array.data", "");
        string[] fieldStructure = fieldPath.Split('.');
        for (int i = 0; i < fieldStructure.Length - 1; i++)
        {
            string txt = fieldStructure[i].BreakDownArrayVariableName(out int ind);
            if (ind >= 0) inObj = GetFieldValueWithIndex(txt, inObj, ind);
            else inObj = GetFieldValue(txt, inObj);
        }

        string fieldName = fieldStructure.Last();

        string text = fieldName.BreakDownArrayVariableName(out int index);

        FieldInfo fieldInfo = GetField(fieldName, inObj, bindings);
        PropertyInfo propInfo = null;
        if (fieldInfo == null)
            propInfo = GetProperty(fieldName, inObj, bindings);

        return new ObjectInfo(fieldInfo, propInfo, inObj, index);
    }

    static ObjectInfo GetCachedInfo(object inObj, string fieldPath)
    {
        ObjectInfo objectInfo;
        if (!cachedObjectInfo.SmartGetValue(inObj, out Dictionary<string, ObjectInfo> dict))
        {
            dict = new Dictionary<string, ObjectInfo>();
            cachedObjectInfo = cachedObjectInfo.CreateAdd(inObj, new Dictionary<string, ObjectInfo>());
        }
        if (!dict.TryGetValue(fieldPath, out objectInfo))
        {
            objectInfo = GetObjectInfo(inObj, fieldPath);
            dict.Add(fieldPath, objectInfo);
        }
        return objectInfo;
    }

    public static Type GetType(object inObj, string fieldPath)
    {
        fieldPath = fieldPath.Replace(".Array.data", "");
        string[] fieldStructure = fieldPath.Split('.');
        Type type = null;
        for (int i = 0; i < fieldStructure.Length; i++)
        {
            string text = fieldStructure[i].BreakDownArrayVariableName(out int index);
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

    private static object GetFieldValue(ObjectInfo info,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        object obj = info.obj;
        int index = info.index;

        FieldInfo field = info.fieldInfo;
        if (field != null)
        {
            if (index < 0)
                return field.GetValue(obj);
            else
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
        }
        else
        {
            PropertyInfo property = info.propInfo;
            if (property != null)
            {

                if (index < 0)
                    return property.GetValue(obj);
                else
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

    public static bool SetFieldValue(ObjectInfo info, object value,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        object obj = info.obj;
        int index = info.index;

        FieldInfo field = info.fieldInfo;
        if (field != null)
        {
            if (index < 0)
            {
                try { Convert.ChangeType(value, field.FieldType); } //WARNING: So ugly...
                catch { return false; }

                field.SetValue(obj, value);
                return true;
            }
            else
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
        }
        else
        {
            PropertyInfo property = info.propInfo;
            if (property != null)
            {
                if (index < 0)
                {
                    try { Convert.ChangeType(value, property.PropertyType); } //WARNING: So ugly...
                    catch { return false; }

                    property.SetValue(obj, value);
                    return true;
                }
                else
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

    public static bool SetValue<T>(SerializedProperty property, T newValue)
    {
        return SetFieldValue(property, newValue);
    }

    public static bool SetFieldValue(SerializedProperty property, object newValue)
    {
        return SetFieldValue(property.serializedObject.targetObject, property.propertyPath, newValue);
    }
#endif
}
