using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#endif

public static class ReflectionTools
{
    static Dictionary<object, Dictionary<string, ObjectInfo>> cachedObjectInfo;
    static Dictionary<object, Dictionary<string, (ObjectInfo, ObjectInfo[])>> cachedObjectInfos;
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

        public MemberInfo MemberInfo()
        {
            if (fieldInfo != null) return fieldInfo;
            else return propInfo;
        }

        public Type Type()
        {
            if (fieldInfo != null) return fieldInfo.FieldType;
            else if (propInfo != null) return propInfo.PropertyType;
            else return null;
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
        string[] fieldStructure = GetFieldStructure(fieldPath);
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

        string[] fieldStructure = GetFieldStructure(fieldPath);

        if (TryGetObjectInfo(ref inObj, ref fieldStructure, out result, bindings))
            return result;
        else return new ObjectInfo();
    }

    static ObjectInfo GetCachedInfo(object inObj, string fieldPath)
    {
        ObjectInfo objectInfo;
        cachedObjectInfo = cachedObjectInfo.CreateIfNull_StaticPersistent();
        if (!cachedObjectInfo.TryGetValue(inObj, out Dictionary<string, ObjectInfo> dict)
            || (dict == null))
        {
            dict = new Dictionary<string, ObjectInfo>();
            cachedObjectInfo.Add(inObj, new Dictionary<string, ObjectInfo>());
        }
        if (!dict.TryGetValue(fieldPath, out objectInfo)
            || (objectInfo.fieldInfo == null))
        {
            objectInfo = GetObjectInfo(inObj, fieldPath);
            if (!objectInfo.IsNull())
                dict.Set(fieldPath, objectInfo);
        }
        return objectInfo;
    }

    public static T[] GetValues<T>(object inObj, string fieldPath, out ObjectInfo arraySource, bool cacheObjectInfo = true)
    {
        object[] obj = GetFieldValues(inObj, fieldPath, out arraySource, cacheObjectInfo);
        if (obj == null) return null;

        T[] result = new T[obj.Length];
        for (int i = 0; i < obj.Length; i++)
        {
            if (obj[i] == null) result[i] = default;
            else result[i] = (T)obj[i];
        }
        return result;
    }

    public static object[] GetFieldValues(object inObj, string fieldPath, out ObjectInfo arraySource, bool cacheObjectInfo = true)
    {
        ObjectInfo[] objectInfos;
        if (!cacheObjectInfo)
            objectInfos = GetObjectInfos(inObj, fieldPath, out arraySource);
        else
            objectInfos = GetCachedInfos(inObj, fieldPath, out arraySource);

        object[] objects = new object[objectInfos.Length];
        for (int i = 0; i < objectInfos.Length; i++)
            objects[i] = GetFieldValue(objectInfos[i]);

        return objects;
    }

    public static bool SetValues<T>(object inObj, string fieldPath, T[] newValues, bool cacheObjectInfo = true)
    {
        object[] objArray = new object[newValues.Length];
        for (int i = 0; i < newValues.Length; i++)
            objArray[i] = newValues[i];
        return SetFieldValues(inObj, fieldPath, objArray, cacheObjectInfo);
    }

    public static bool SetFieldValues(object inObj, string fieldPath, object[] newValues, bool cacheObjectInfo = true)
    {
        ObjectInfo arraySourceInfo;
        ObjectInfo[] objectInfos;
        if (!cacheObjectInfo)
            objectInfos = GetObjectInfos(inObj, fieldPath, out arraySourceInfo);
        else
            objectInfos = GetCachedInfos(inObj, fieldPath, out arraySourceInfo);

        bool wasSet = false;
        for (int i = 0; i < objectInfos.Length; i++)
            if (SetFieldValue(objectInfos[i], newValues[i]))
                wasSet = true;

        return wasSet;
    }

    public static ObjectInfo[] GetObjectInfos(object inObj, string fieldPath, out ObjectInfo arraySource)
    {
        if (cachedObjectInfos.SmartGetValue(inObj, fieldPath, out (ObjectInfo, ObjectInfo[]) tupla))
        {
            arraySource = tupla.Item1;
            return tupla.Item2;
        }

        string[] fieldStructure = GetFieldStructure(fieldPath);

        bool completedPath = TryGetObjectInfo(ref inObj, ref fieldStructure, out arraySource);
        if (arraySource.Type().IsOrInheritsFrom(typeof(ICollection)))
        {
            if (GetFieldValue(arraySource) is not ICollection collection)
                return new ObjectInfo[0];

            ObjectInfo[] result = new ObjectInfo[collection.Count];
            for (int i = 0; i < result.Length; i++)
            {
                ObjectInfo subObj = new ObjectInfo(arraySource.fieldInfo, arraySource.propInfo, arraySource.obj, i);
                if (completedPath)
                    result[i] = subObj;
                else
                {
                    string[] propStruct = fieldStructure.CreateCopy();
                    if (TryGetObjectInfo(ref subObj.obj, ref propStruct, out subObj))
                        result[i] = subObj;
                }
            }
            return result;
        }
        else return null;
    }

    static ObjectInfo[] GetCachedInfos(object inObj, string fieldPath, out ObjectInfo arraySource)
    {
        cachedObjectInfos = cachedObjectInfos.CreateIfNull_StaticPersistent();
        if (!cachedObjectInfos.TryGetValue(inObj, out Dictionary<string, (ObjectInfo, ObjectInfo[])> dict)
            || (dict == null))
        {
            dict = new Dictionary<string, (ObjectInfo, ObjectInfo[])>();
            cachedObjectInfos.Add(inObj, new Dictionary<string, (ObjectInfo, ObjectInfo[])>());
        }
        if (!dict.TryGetValue(fieldPath, out (ObjectInfo, ObjectInfo[]) objectInfos)
            || (objectInfos.Item1.fieldInfo == null) || objectInfos.Item2.IsNullOrEmpty())
        {
            objectInfos.Item2 = GetObjectInfos(inObj, fieldPath, out objectInfos.Item1);
            if (!(objectInfos.Item1.IsNull() || objectInfos.Item2.IsNullOrEmpty()))
                dict.Set(fieldPath, objectInfos);
        }
        arraySource = objectInfos.Item1;
        return objectInfos.Item2;
    }

    static bool TryGetObjectInfo(
        ref object parent, ref string[] propStructure, out ObjectInfo info,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        if (propStructure.IsNullOrEmpty())
        {
            info = new ObjectInfo(null, null, parent, -1);
            return true;
        }

        FieldInfo field = null;
        PropertyInfo property = null;
        int index = -1;

        int i = 0;
        object next = parent;
        for (; i < propStructure.Length; i++)
        {
            if (next == null) break;
            parent = next;
            next = null;

            string name = propStructure[i].BreakDownArrayVariableName(out index);

            field = GetField(name, parent, bindings);
            property = null;
            if (field != null)
                next = GetFieldValue(field, parent, index);
            else
            {
                property = GetProperty(name, parent, bindings);
                if (property != null)
                    next = GetPropertyValue(property, parent, index);
            }
        }

        info = new ObjectInfo(field, property, parent, index);

        if (i >= propStructure.Length)
        {
            propStructure = new string[0];
            return true;
        }
        else
        {
            string[] newPropStructure = new string[propStructure.Length - i];
            for (int j = 0; j < newPropStructure.Length; j++)
                newPropStructure[j] = propStructure[i + j];
            propStructure = newPropStructure;
            return false;
        }
    }

    public static Type GetType(object inObj, string fieldPath)
    {
        string[] fieldStructure = GetFieldStructure(fieldPath);
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

    public static MemberInfo GetMemberInfo(object inObj, string fieldPath, bool cacheObjectInfo = true,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        ObjectInfo objectInfo;
        if (!cacheObjectInfo)
            objectInfo = GetObjectInfo(inObj, fieldPath, bindings);
        else
            objectInfo = GetCachedInfo(inObj, fieldPath); //TO DO: Bindings??
        return objectInfo.MemberInfo();
    }

    static FieldInfo GetField(string fieldName, object obj,
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

    static PropertyInfo GetProperty(string fieldName, object obj,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        Type type = obj?.GetType();
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

    public static MethodInfo GetMethodInfo(string methodName, object obj,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        return obj.GetType().GetMethod(methodName, bindings);
    }

    public static T GetDelegate<T>(string methodName, object obj,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        where T : Delegate
    {
        MethodInfo methodInfo = GetMethodInfo(methodName, obj, bindings);
        return methodInfo.CreateDelegate(typeof(T), obj) as T;
    }

    public static object GetFieldValue(string fieldName, object obj,
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

    public static object GetFieldValue(string fieldName, object obj, int index,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        FieldInfo field = GetField(fieldName, obj, bindings);
        if (field != null)
            return GetFieldValue(field, obj, index, bindings);
        else
        {
            PropertyInfo property = GetProperty(fieldName, obj, bindings);
            if (property != null)
                return GetPropertyValue(property, obj, index, bindings);
        }
        return default;
    }

    static object GetFieldValue(FieldInfo fieldInfo, object obj, int index,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        if (fieldInfo != null)
        {
            object list = fieldInfo.GetValue(obj);
            if (index < 0)
                return list;
            else if ((list is IList l) && index.IsBetween(0, l.Count))
                return l[index];
            else return null;
        }
        else return default;
    }

    static object GetPropertyValue(PropertyInfo propertyInfo, object obj, int index,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        if (propertyInfo != null)
        {
            object list = propertyInfo.GetValue(obj);
            if (index < 0)
                return list;
            else if ((list is IList l) && index.IsBetween(0, l.Count))
                return l[index];
            else return null;
        }
        else return default;
    }

    public static object GetFieldValue(ObjectInfo info,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        object obj = info.obj;
        int index = info.index;

        FieldInfo field = info.fieldInfo;
        if (field != null)
            return GetFieldValue(field, obj, index, bindings);
        else
        {
            PropertyInfo property = info.propInfo;
            if (property != null)
                return GetPropertyValue(property, obj, index, bindings);
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

    public static bool SetFieldValue(string fieldName, object obj, int index, object value,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        FieldInfo field = GetField(fieldName, obj, bindings);
        if (field != null)
            return SetFieldValue(field, obj, index, value, bindings);
        else
        {
            PropertyInfo property = GetProperty(fieldName, obj, bindings);
            if (property != null)
                return SetPropertyValue(property, obj, index, value, bindings);
        }
        return false;
    }

    static bool SetFieldValue(FieldInfo fieldInfo, object obj, int index, object value,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        if (fieldInfo != null)
        {
            if (index < 0)
            {
                try { Convert.ChangeType(value, fieldInfo.FieldType); } //WARNING: So ugly...
                catch { return false; }

                fieldInfo.SetValue(obj, value);
                return true;
            }
            else
            {
                object list = fieldInfo.GetValue(obj);
                if ((list is IList l) && index.IsBetween(0, l.Count))
                {
                    //TO DO: Try convert the type
                    l[index] = value;
                    fieldInfo.SetValue(obj, l);
                    return true;
                }
                else return false;
            }
        }
        else return false;
    }

    static bool SetPropertyValue(PropertyInfo propertyInfo, object obj, int index, object value,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        if (propertyInfo != null)
        {
            object list = propertyInfo.GetValue(obj);
            if (index < 0)
            {
                try { Convert.ChangeType(value, propertyInfo.PropertyType); } //WARNING: So ugly...
                catch { return false; }

                list = value;
                return true;
            }
            else if ((list is IList l) && index.IsBetween(0, l.Count))
            {
                //TO DO: Try convert the type
                l[index] = value;
                return true;
            }
            else return false;
        }
        else return false;
    }

    public static bool SetFieldValue(ObjectInfo info, object value,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        object obj = info.obj;
        int index = info.index;

        FieldInfo field = info.fieldInfo;
        if (field != null)
            return SetFieldValue(field, obj, index, value, bindings);
        else
        {
            PropertyInfo property = info.propInfo;
            if (property != null)
                return SetPropertyValue(property, obj, index, value, bindings);
        }
        return false;
    }

    public static T InvokePrivateMethod<T>(object instance, string name, params object[] parameters)
    {
        MethodInfo method = GetMethod(instance.GetType(), name, parameters, BindingFlags.NonPublic | BindingFlags.Instance);
        if (method.IsGenericMethod)
            method = method.MakeGenericMethod(typeof(T));

        return (T)method.Invoke(instance, parameters);
    }

    public static T InvokePrivateMethod<T>(Type type, string name, params object[] parameters)
    {
        MethodInfo method = GetMethod(type, name, parameters, BindingFlags.NonPublic | BindingFlags.Static);
        if (method.IsGenericMethod)
            method = method.MakeGenericMethod(typeof(T));

        return (T)method.Invoke(null, parameters);
    }

    public static T InvokeMethod<T>(object instance, string name, params object[] parameters)
    {
        MethodInfo method = GetMethod(instance.GetType(), name, parameters, BindingFlags.Public | BindingFlags.Instance);
        if (method.IsGenericMethod)
            method = method.MakeGenericMethod(typeof(T));

        return (T)method.Invoke(instance, parameters);
    }

    public static T InvokeMethod<T>(Type type, string name, params object[] parameters)
    {
        return InvokeMethod<T>(type, name, new Type[] { typeof(T) }, parameters);
    }

    public static T InvokeMethod<T>(Type type, string name, Type[] genericTypes, params object[] parameters)
    {
        MethodInfo method = GetMethod(type, name, parameters, BindingFlags.Public | BindingFlags.Static);
        if (method.IsGenericMethod)
            method = method.MakeGenericMethod(genericTypes);

        return (T)method.Invoke(null, parameters);
    }

    public static void InvokePrivateMethod(object instance, string name, params object[] parameters)
    {
        MethodInfo method = GetMethod(instance.GetType(), name, parameters, BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(instance, parameters);
    }

    public static void InvokePrivateMethod(Type type, string name, params object[] parameters)
    {
        MethodInfo method = GetMethod(type, name, parameters, BindingFlags.NonPublic | BindingFlags.Static);
        method.Invoke(null, parameters);
    }

    public static void InvokeMethod(object instance, string name, params object[] parameters)
    {
        MethodInfo method = GetMethod(instance.GetType(), name, parameters, BindingFlags.Public | BindingFlags.Instance);
        method.Invoke(instance, parameters);
    }

    public static void InvokeMethod(Type type, string name, params object[] parameters)
    {
        MethodInfo method = GetMethod(type, name, parameters, BindingFlags.Public | BindingFlags.Static);
        method.Invoke(null, parameters);
    }

    static MethodInfo GetMethod(Type type, string name, object[] parameters, BindingFlags flags)
    {
        Type[] paramTypes = parameters.Select(p => p.GetType()).ToArray();
        MethodInfo method = type.GetMethod(name, flags, null, paramTypes, null);

        if (method == null)
        {
            string paramNames = string.Join(", ", paramTypes.Select(t => t.Name).ToArray());
            throw new InvalidOperationException(type.Name + "." + name + "<T>(" + paramNames + ") wasn't found.");
        }

        return method;
    }

    static string[] GetFieldStructure(string fieldPath)
    {
        fieldPath = fieldPath.Replace(".Array.data[", "[");
        return fieldPath.Split('.');
    }

    public static T Clone<T>(T original)
    {
        if (original == null)
            return default;

        return (T)original.GetType()
        .GetMethod(
            "MemberwiseClone",
            BindingFlags.Instance | BindingFlags.NonPublic
        )
        .Invoke(original, null);
    }

    public static int GetSettingsHash<T>(T component, bool includePropertyInfos = false) where T : Component
    {
        if (component == null)
            return 0;

        Type type = component.GetType();
        MemberInfo[] members = type.GetMembers();
        List<object> values = new();
        foreach (MemberInfo member in members)
        {
            object value;
            if (member is FieldInfo field)
                value = field.GetValue(component);
            else if (includePropertyInfos && (member is PropertyInfo property))
                value = property.GetValue(component);
            else continue;

            if (value != null)
                values.Add(value);
        }
        return HashMaker.Elements(values.ToArray());
    }

#if UNITY_EDITOR
    public static T GetValue<T>(SerializedProperty property)
    {
        return (T)GetFieldValue(property);
    }

    public static object GetFieldValue(SerializedProperty property)
    {
        if (property.propertyPath.IsNullOrEmpty())
            return property.serializedObject.targetObject;
        else return GetFieldValue(property.serializedObject.targetObject, property.propertyPath);
    }

    public static bool SetValue<T>(SerializedProperty property, T newValue)
    {
        return SetFieldValue(property, newValue);
    }

    public static bool SetFieldValue(SerializedProperty property, object newValue)
    {
        return SetFieldValue(property.serializedObject.targetObject, property.propertyPath, newValue);
    }

    public static SerializedProperty FirstSerializableInPath(SerializedProperty source, ref string[] propStructure)
    {
        return FirstSerializableInPath(source.serializedObject.targetObject, ref propStructure, source.propertyPath);
    }

    public static SerializedProperty FirstSerializableInPath(object source, ref string[] propStructure, string initialPath = "")
    {
        string path = initialPath;
        SerializedObject serializedObj = (source is Object obj) ? new SerializedObject(obj) : null;
        for (int i = 0; i < propStructure.Length; i++)
        {
            path += (path.IsNullOrEmpty() ? "" : ".") + propStructure[i];

            if (serializedObj != null)
            {
                SerializedProperty sp = serializedObj.FindProperty(path);
                if (sp != null)
                {
                    int next = i + 1;
                    string[] remaining = new string[propStructure.Length - next];
                    for (int j = 0; j < remaining.Length; j++)
                        remaining[j] = propStructure[next + j];
                    propStructure = remaining;
                    return sp;
                }
            }

            source = GetFieldValue(source, path);
            if (source is Object uobj)
            {
                SerializedObject so = new SerializedObject(uobj);
                if (so != null)
                {
                    serializedObj = so;
                    path = "";
                }
            }
        }
        return null;
    }
#endif
}
