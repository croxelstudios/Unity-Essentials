#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using System;
using Object = UnityEngine.Object;

public struct SerializedPropertyData : IEquatable<SerializedPropertyData>
{
    public Object obj;
    public string path;

    public SerializedPropertyData(Object obj, string path)
    {
        this.obj = obj;
        this.path = path;
    }

    public SerializedPropertyData(SerializedProperty property)
    {
        obj = property.serializedObject.targetObject;
        path = property.propertyPath;
    }

    public SerializedProperty GetSerializedProperty()
    {
        SerializedObject serializedObject = GetSerializedObject();
        if (serializedObject == null || string.IsNullOrEmpty(path)) return null;
        return serializedObject.FindProperty(path);
    }

    public SerializedObject GetSerializedObject()
    {
        if (obj == null) return null;
        else return new SerializedObject(obj);
    }

    public void UpdateHashCode()
    {

    }

    public override bool Equals(object other)
    {
        if (!(other is SerializedPropertyData)) return false;
        return Equals((SerializedPropertyData)other);
    }

    public bool Equals(SerializedPropertyData other)
    {
        return (obj == other.obj)
            && (path == other.path);
    }

    public override int GetHashCode()
    {
        return ((obj == null) ? 0 : obj.GetHashCode()) * 31
            + ((path == null) ? 0 : path.GetHashCode());
    }

    public static bool operator ==(SerializedPropertyData o1, SerializedPropertyData o2)
    {
        return o1.Equals(o2);
    }

    public static bool operator !=(SerializedPropertyData o1, SerializedPropertyData o2)
    {
        return !o1.Equals(o2);
    }
}
#endif
