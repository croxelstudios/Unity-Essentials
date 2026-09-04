using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
#endif

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class GreySuffixAttribute : Attribute
{
    public string Suffix;
    public string Condition;
    public GreySuffixAttribute(string suffix, string condition = "")
    {
        Suffix = suffix;
        Condition = condition;
    }
}

#if UNITY_EDITOR
public class GreySuffixAttributeDrawer<T> : OdinAttributeDrawer<GreySuffixAttribute, T>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        bool shouldShow = true;

        if (!string.IsNullOrEmpty(Attribute.Condition))
        {
            try
            {
                shouldShow = ValueResolver.Get<bool>(Property, Attribute.Condition).GetValue();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"LabelInlineText condition resolution failed:" +
                    $" '{Attribute.Condition}'. Exception: {ex.Message}");
            }
        }

        if (shouldShow)
        {
            EditorGUILayout.BeginHorizontal();

            CallNextDrawer(label);

            Rect controlRect = EditorGUI.IndentedRect(GUILayoutUtility.GetLastRect());
            float reservedLabelWidth = EditorGUIUtility.labelWidth;
            float labelSize = Mathf.Min(
                EditorStyles.label.CalcSize(new GUIContent(label.text)).x, reservedLabelWidth);

            float spaceBetween = reservedLabelWidth - labelSize;
            string text = TruncateStringToFitWidth(
                Attribute.Suffix, EditorStyles.miniLabel, spaceBetween);
            float size = EditorStyles.miniLabel.CalcSize(new GUIContent(Attribute.Suffix)).x;
            Rect suffixRect = new Rect(
                controlRect.x + labelSize, controlRect.y, size, controlRect.height);

            Color oldContentColor = GUI.contentColor;
            GUI.contentColor = Color.grey;
            GUI.Label(suffixRect, text, EditorStyles.miniLabel);
            GUI.contentColor = oldContentColor;

            EditorGUILayout.EndHorizontal();
        }
        else CallNextDrawer(label);
    }

    static string TruncateStringToFitWidth(string text, GUIStyle style, float maxWidth)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // si cabe entero, devolvemos tal cual
        if (style.CalcSize(new GUIContent(text)).x <= maxWidth) return text;

        // reserva espacio para la elipsis
        string ellipsis = "…";
        float ellipsisW = style.CalcSize(new GUIContent(ellipsis)).x;
        if (ellipsisW >= maxWidth) return ""; // no cabe ni la elipsis

        // binary search sobre la longitud máxima
        int lo = 0;
        int hi = text.Length;
        int best = 0;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            string candidate = text.Substring(0, mid) + ellipsis;
            float w = style.CalcSize(new GUIContent(candidate)).x;

            if (w <= maxWidth)
            {
                best = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        if (best <= 0) return ""; // no cabe nada útil
        return text.Substring(0, best) + ellipsis;
    }
}
#endif

