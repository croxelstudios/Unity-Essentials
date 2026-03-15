#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

//TO DO: Support for getting an array of properties from a single property that is
//inside an array of refernces
public static class ObjectExtension_GetSerializedProperty
{
    public static SerializedProperty GetSerializedProperty(
        this SerializedObject serializedObject, string compoundName)
    {
        string[] propStructure = compoundName.Split('.');
        SerializedProperty prop = null;

        for (int i = 0; i < propStructure.Length; i++)
        {
            string text = propStructure[i].BreakDownArrayVariableName(out int index);

            if (prop == null) prop = serializedObject.FindProperty(text);
            else
            {
                SerializedProperty newProp = prop.FindPropertyRelative(text);
                if (newProp == null)
                {
                    serializedObject = new SerializedObject(prop.objectReferenceValue);
                    newProp = serializedObject.FindProperty(text);
                }
                prop = newProp;
            }

            if ((index >= 0) && (prop != null)) prop = prop.GetArrayElementAtIndex(index);
        }

        return prop;
    }

    public static SerializedProperty GetSerializedProperty(
        this Object obj, string compoundName)
    {
        SerializedObject serializedObject = new SerializedObject(obj);
        return serializedObject.GetSerializedProperty(compoundName);
    }
}
#endif
