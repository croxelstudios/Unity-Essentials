using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

public class CustomTag_Colored : CustomTag
{
    [SerializeField]
    [OnValueChanged("UpdateTagList")]
    StringList_Colored coloredTagList;
    [SerializeField]
    DXIntEvent launchTagID = null;
    [SerializeField]
    DXColorEvent launchColor = null;

    public void UpdateTagList()
    {
        tagList = coloredTagList;
        LaunchEvents();
    }

    public void LaunchEvents()
    {
        launchTagID?.Invoke(customTag);
        launchColor?.Invoke(coloredTagList.coloredTags[customTag].color);
    }

    [StringPopup(new string[] { "tagList", "tags" })]
    public override void SwitchTag(int newTag)
    {
        base.SwitchTag(newTag);
        LaunchEvents();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CustomTag_Colored))]
public class CustomTag_Colored_Inspector : OdinEditor
{
    InspectorProperty coloredTagList;
    SerializedProperty customTag;
    InspectorProperty launchTagID;
    InspectorProperty launchColor;

    protected override void OnEnable()
    {
        coloredTagList = Tree.GetPropertyAtPath("coloredTagList");
        customTag = serializedObject.FindProperty("customTag");
        launchTagID = Tree.GetPropertyAtPath("launchTagID");
        launchColor = Tree.GetPropertyAtPath("launchColor");
    }

    public override void OnInspectorGUI()
    {
        Tree.BeginDraw(true);
        serializedObject.DrawMScriptField();
        bool shouldLaunchEvents = false;
        EditorGUI.BeginChangeCheck();
        coloredTagList.Draw();
        EditorGUILayout.PropertyField(customTag);
        launchTagID.Draw();
        launchColor.Draw();
        if (EditorGUI.EndChangeCheck()) shouldLaunchEvents = true;
        Tree.EndDraw();
        if (shouldLaunchEvents)
            ((CustomTag_Colored)target).LaunchEvents();
    }
}
#endif
