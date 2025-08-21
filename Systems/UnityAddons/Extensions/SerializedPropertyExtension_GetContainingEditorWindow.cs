#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

public static class SerializedPropertyExtension_GetContainingEditorWindow
{
    public static EditorWindow GetContainingEditorWindow(this SerializedProperty property)
    {
        Editor refEd = null;
        return GetContainingEditorWindow(property, ref refEd);
    }

    public static EditorWindow GetContainingEditorWindow(this SerializedProperty property, ref Editor editor)
    {
        var editorWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();

        foreach (var window in editorWindows)
        {
            var trackerField = window.GetType().GetField("m_Tracker", BindingFlags.NonPublic | BindingFlags.Instance);
            if (trackerField != null)
            {
                var tracker = trackerField.GetValue(window);
                if (tracker != null)
                {
                    Editor[] activeEditors = ((ActiveEditorTracker)tracker).activeEditors;
                    if (activeEditors != null)
                        foreach (var edit in activeEditors)
                            if (edit.target == property.serializedObject.targetObject)
                            {
                                editor = edit;
                                return window;
                            }
                }
            }
        }

        return null;
    }
}
#endif
