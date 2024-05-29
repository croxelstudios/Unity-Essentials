using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public static class EditorGUILayoutExtension_DrawMScriptField
{
    public static void DrawMScriptField(this SerializedObject serializedObject)
    {
        SerializedProperty prop = serializedObject.FindProperty("m_Script");
        using (new EditorGUI.DisabledScope("m_Script" == prop.propertyPath))
            EditorGUILayout.PropertyField(prop);
    }
}
#endif
