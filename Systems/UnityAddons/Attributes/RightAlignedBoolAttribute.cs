using System;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class RightAlignAttribute : Attribute
{
    public float width;

    public RightAlignAttribute(float width)
    {
        this.width = width;
    }

    public RightAlignAttribute(int width)
    {
        this.width = width;
    }
}

#if UNITY_EDITOR
public sealed class RightAlignValueAttributeDrawer<T> : OdinAttributeDrawer<RightAlignAttribute, T>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        float width = Attribute.width;

        if (width >= 0f)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (label != null)
                    GUILayout.Label(label, EditorStyles.label, GUILayout.ExpandWidth(true), GUILayout.MinWidth(0f));
                else GUILayout.FlexibleSpace();

                using (new GUILayout.VerticalScope(GUILayout.Width(width)))
                    CallNextDrawer(null);
            }
        }
        else CallNextDrawer(label);
    }
}
#endif
