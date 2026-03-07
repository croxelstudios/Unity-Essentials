using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//TO DO: A bunch of things can be optimized here now that I have ObjectExtension_GetSerializedProperty.
//Also need to check StringSelectorAttribute.
public class BasePropertyRefAttribute : PropertyAttribute
{
    public string propPath;

    public BasePropertyRefAttribute(string propPath)
    {
        this.propPath = propPath;
    }

    public object GetSubObject(Object targetObj, SerializedProperty property)
    {
        string[] pathSegments = property.propertyPath.Split(".");
        string path = "";
        int stop;
        if ((pathSegments.Length > 1) && (pathSegments[pathSegments.Length - 2] == "Array"))
            stop = pathSegments.Length - 3;
        else stop = pathSegments.Length - 1;
        for (int i = 0; i < stop; i++)
        {
            if (i != 0) path += ".";
            path += pathSegments[i];
        }
        return (path != "") ? ReflectionTools.GetFieldValue(targetObj, path) : targetObj;
    }

    public T GetValue<T>(object targetObj) where T : class
    {
        if (!propPath.IsNullOrEmpty())
        {
            object obj = ReflectionTools.GetFieldValue(targetObj, propPath);
            T result = obj as T;
            return result;
        }
        else return null;
    }

    public T GetValue<T>(Object targetObj, SerializedProperty property) where T : class
    {
        return GetValue<T>(GetSubObject(targetObj, property));
    }
}
