using UnityEngine;
using System;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

[ExecuteAlways]
[CreateAssetMenu(menuName = "Croxel Scriptables/String List (Colored)")]
public class StringList_Colored : StringList
{
    public ColoredTag[] coloredTags = new ColoredTag[] { new ColoredTag("White", Color.white) };

#if UNITY_EDITOR
    void OnValidate()
    {
        UpdateArrays();
    }

    public void UpdateArrays()
    {
        if (coloredTags != null)
        {
            tags = new string[coloredTags.Length];
            for (int i = 0; i < coloredTags.Length; i++)
                tags[i] = coloredTags[i].tag;
        }
        else tags = null;
    }
#endif

    [Serializable]
    public struct ColoredTag
    {
        [HorizontalGroup]
        public string tag;
        [HorizontalGroup]
        [HideLabel]
        public Color color;

        public ColoredTag(string tag, Color color)
        {
            this.tag = tag;
            this.color = color;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(StringList_Colored))]
public class CustomTags_Colored_Inspector : OdinEditor
{
    InspectorProperty prop;

    protected override void OnEnable()
    {
        prop = Tree.GetPropertyAtPath("coloredTags");
    }

    public override void OnInspectorGUI()
    {
        Tree.BeginDraw(true);
        prop.Draw();
        Tree.EndDraw();
    }
}
#endif
