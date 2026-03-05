#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class SerializedPropertyExtension_PrefabOverrideRendering
{
    public static GUIStyle PrefabOverrideRendering(this SerializedProperty property, Rect argRect)
    {
        GUIStyle labelStyle = EditorStyles.label;
        if (property != null)
        {
            Object[] objs = property.serializedObject.targetObjects;
            if (objs.Length == 1) //Prefab override rendering
            {
                bool hasPrefabOverride = property.prefabOverride;
                //if (!linkedProperties || hasPrefabOverride)
                //    EditorGUIUtility.SetBoldDefaultFont(hasPrefabOverride);
                if (hasPrefabOverride && !property.isDefaultOverride/* && !property.isDrivenRectTransformProperty*/)
                {
                    Rect highlightRect = argRect;
                    highlightRect.xMin += EditorGUI.indentLevel;

                    highlightRect.x = 0f;
                    highlightRect.width = 2;
                    Graphics.DrawTexture(highlightRect, EditorGUIUtility.whiteTexture,
                        new Rect(), 0, 0, 0, 0, GUIInternalConstants.prefabOverrideColor,
                        new Material(EditorGUIUtility.LoadRequired(GUIInternalConstants.prefabOverrideShaderPath) as Shader));
                    labelStyle = EditorStyles.boldLabel;
                }
            }
        }
        return labelStyle;
    }
}
#endif
