using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
public static class SerializedPropertyExtension_OnGUIChildDraw
{
    public static void OnGUIChildDraw(this SerializedProperty property, GUIContent label = null)
    {
        GUIContent lb = (label != null) ? label : new GUIContent(property.displayName);
        float height = EditorGUI.GetPropertyHeight(property, true);
        Rect rect = EditorGUILayout.GetControlRect(true, height);
        EditorGUI.BeginProperty(rect, lb, property);
        EditorGUI.PropertyField(rect, property, lb);
        EditorGUI.EndProperty();
        property.serializedObject.ApplyModifiedProperties();
        PropertyMenuInterceptor.CheckRightClickArea(rect, property);
    }
}
#endif
