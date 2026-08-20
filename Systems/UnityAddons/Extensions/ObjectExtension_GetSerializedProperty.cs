#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ObjectExtension_GetSerializedProperty
{
    public static SerializedProperty GetSerializedProperty(
        this SerializedProperty serializedProperty, string compoundName, bool useReflectionToSkipNonSerialized = true)
    {
        string[] propStructure = GetPropStructure(ref serializedProperty, compoundName);
        return GetSerializedProperty(serializedProperty, propStructure, useReflectionToSkipNonSerialized);
    }

    public static SerializedProperty GetSerializedProperty(
        this SerializedObject serializedObject, string compoundName, bool useReflectionToSkipNonSerialized = true)
    {
        return serializedObject.GetIterator().GetSerializedProperty(compoundName, useReflectionToSkipNonSerialized);
    }

    public static SerializedProperty GetSerializedProperty(
        this Object obj, string compoundName, bool useReflectionToSkipNonSerialized = true)
    {
        SerializedObject serializedObject = new SerializedObject(obj);
        return serializedObject.GetSerializedProperty(compoundName, useReflectionToSkipNonSerialized);
    }

    public static SerializedProperty GetSerializedProperty(
        this SerializedProperty serializedProperty, string[] propStructure, bool useReflectionToSkipNonSerialized = true)
    {
        if (TryGetSerializedProperty(ref serializedProperty, ref propStructure))
            return serializedProperty;
        else if (useReflectionToSkipNonSerialized && !propStructure.IsNullOrEmpty())
            return ReflectionTools.FirstSerializableInPath(serializedProperty, ref propStructure)?.
                GetSerializedProperty(propStructure, true);
        else return null;
    }

    static bool TryGetSerializedProperty(
        ref SerializedProperty serializedProperty, ref string[] propStructure)
    {
        if (propStructure.IsNullOrEmpty())
            return true;

        int i = 0;
        for (; i < propStructure.Length; i++)
        {
            string text = propStructure[i].BreakDownArrayVariableName(out int index);

            SerializedProperty next = serializedProperty.FindProperty(text);
            if (next == null) break;
            else serializedProperty = next;

            if (index >= 0)
                next = serializedProperty.GetArrayElementAtIndex(index);

            if (next == null) break;
            else serializedProperty = next;
        }

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

    static string[] GetPropStructure(ref SerializedProperty serializedProperty, string compoundName)
    {
        int upParents = compoundName.Occurrences("/.");
        compoundName = compoundName.Replace("/.", "");
        for (int i = 0; i < upParents; i++)
            serializedProperty = serializedProperty.GetParent();

        compoundName = compoundName.Replace(".Array.data[", "[");
        return compoundName.Split('.');
    }

    public static SerializedProperty[] GetSerializedProperties(this SerializedProperty serializedProperty, string compoundName,
        out SerializedProperty arraySource, bool useReflectionToSkipNonSerialized = true)
    {
        string[] propStructure = GetPropStructure(ref serializedProperty, compoundName);
        return serializedProperty.GetSerializedProperties(propStructure, out arraySource, useReflectionToSkipNonSerialized);
    }

    public static SerializedProperty[] GetSerializedProperties(this SerializedProperty serializedProperty, string[] propStructure,
        out SerializedProperty arraySource, bool useReflectionToSkipNonSerialized = true)
    {
        arraySource = null;

        bool completedPath = TryGetSerializedProperty(ref serializedProperty, ref propStructure);
        bool tryReflection = false;
        if (serializedProperty.isArray)
        {
            arraySource = serializedProperty;

            SerializedProperty[] result = new SerializedProperty[serializedProperty.arraySize];
            for (int i = 0; i < result.Length; i++)
            {
                SerializedProperty subProp = serializedProperty.GetArrayElementAtIndex(i);
                if (completedPath)
                    result[i] = subProp;
                else
                {
                    string[] propStruct = propStructure.CreateCopy();
                    if (TryGetSerializedProperty(ref subProp, ref propStruct))
                        result[i] = subProp;
                    else if (useReflectionToSkipNonSerialized && !propStruct.IsNullOrEmpty())
                        result[i] = ReflectionTools.FirstSerializableInPath(subProp, ref propStruct)?.
                            GetSerializedProperty(propStruct, true);
                }
            }
            return result;
        }
        else tryReflection = true;

        if (tryReflection && useReflectionToSkipNonSerialized && !propStructure.IsNullOrEmpty())
            return ReflectionTools.FirstSerializableInPath(serializedProperty, ref propStructure)?.
                GetSerializedProperties(propStructure, out arraySource, true);

        return null;
    }

    public static SerializedProperty[] GetSerializedProperties(this SerializedObject serializedObject, string compoundName, out SerializedProperty arraySource)
    {
        return serializedObject.GetIterator().GetSerializedProperties(compoundName, out arraySource);
    }

    public static SerializedProperty[] GetSerializedProperties(this Object obj, string compoundName, out SerializedProperty arraySource)
    {
        SerializedObject serializedObject = new SerializedObject(obj);
        return serializedObject.GetSerializedProperties(compoundName, out arraySource);
    }

    public static SerializedProperty[] GetSerializedProperties(this SerializedProperty serializedProperty, string compoundName)
    {
        return serializedProperty.GetSerializedProperties(compoundName, out SerializedProperty foo);
    }

    public static SerializedProperty[] GetSerializedProperties(this SerializedObject serializedObject, string compoundName)
    {
        return serializedObject.GetSerializedProperties(compoundName, out SerializedProperty foo);
    }

    public static SerializedProperty[] GetSerializedProperties(this Object obj, string compoundName)
    {
        return obj.GetSerializedProperties(compoundName, out SerializedProperty foo);
    }
}
#endif
