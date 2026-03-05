#if UNITY_EDITOR
using UnityEngine;

using UnityEditor;

public static class SerializedPropertyExtension_PropertyField
{
    public static void PropertyField(this SerializedProperty element, string displayName, float y, Rect rect, float widthSum = -10f, float widthMult = 1f)
    {
        EditorGUI.PropertyField(
                       new Rect(rect.x + 10, y,
                       (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                       element, new GUIContent(displayName), true);
    }

    public static void PropertyField(this SerializedProperty element, float y, Rect rect, float widthSum = -10f, float widthMult = 1f)
    {
        EditorGUI.PropertyField(
                       new Rect(rect.x + 10, y,
                       (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                       element, new GUIContent(element.displayName), true);
    }

    public static void PropertyField(this SerializedProperty element, string displayName, Rect rect, float widthSum = -10f, float widthMult = 1f)
    {
        EditorGUI.PropertyField(
                       new Rect(rect.x + 10, rect.y,
                       (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                       element, new GUIContent(displayName), true);
    }

    public static void PropertyField(this SerializedProperty element, Rect rect, float widthSum = -10f, float widthMult = 1f)
    {
        EditorGUI.PropertyField(
                       new Rect(rect.x + 10, rect.y,
                       (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                       element, new GUIContent(element.displayName), true);
    }
}
#endif
