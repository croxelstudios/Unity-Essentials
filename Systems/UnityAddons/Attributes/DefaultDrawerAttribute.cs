using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

public class DefaultDrawerAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(DefaultDrawerAttribute))]
public class DefaultDrawerAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label);
    }
}
#endif
