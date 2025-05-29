using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Reflection;
#endif

public class ChangeCheckAttribute : PropertyAttribute
{
    public string methodName;
    public object[] args;

    public ChangeCheckAttribute(string methodName)
    {
        this.methodName = methodName;
        this.args = null;
    }

    public ChangeCheckAttribute(string methodName, object[] args)
    {
        this.methodName = methodName;
        this.args = args;
    }
}

public class BeginChangeCheckAttribute : PropertyAttribute
{
}

public class EndChangeCheckAttribute : PropertyAttribute
{
    public string methodName;
    public object[] args;

    public EndChangeCheckAttribute(string methodName)
    {
        this.methodName = methodName;
        this.args = null;
    }

    public EndChangeCheckAttribute(string methodName, object[] args)
    {
        this.methodName = methodName;
        this.args = args;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ChangeCheckAttribute))]
public class ChangeCheckAttribute_Drawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        bool prev = property.isExpanded;
        EditorGUI.PropertyField(position, property, label, true);
        if (EditorGUI.EndChangeCheck() && (property.isExpanded == prev))
        {
            ChangeCheckAttribute field = attribute as ChangeCheckAttribute;
            property.serializedObject.targetObject.GetType().GetMethod(field.methodName)?
                .Invoke(property.serializedObject.targetObject, field.args);
        }
    }
}

[CustomPropertyDrawer(typeof(BeginChangeCheckAttribute))]
public class BeginChangeCheckAttribute_Drawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, property, label);
    }
}

[CustomPropertyDrawer(typeof(EndChangeCheckAttribute))]
public class EndChangeCheckAttribute_Drawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label, true);
        if (EditorGUI.EndChangeCheck())
        {
            EndChangeCheckAttribute field = attribute as EndChangeCheckAttribute;
            property.serializedObject.targetObject.GetType().GetMethod(field.methodName)?
                .Invoke(property.serializedObject.targetObject, field.args);
        }
    }
}
#endif
