using UnityEngine;
using QFSW.QC;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/StringSignal")] //Change type here
public class StringSignal : ValueSignal<string> //Change type here
{
    [SerializeField]
    bool isScenePath = false;
#if UNITY_EDITOR
    [SerializeField]
    [ShowIf("MustShowStartValue")]
    [ShowIf("isScenePath")]
    [LabelText("Scene")]
    SceneReference startScene = default;
#endif
    [HideIf("MustShowStartValue")]
    [OnValueChanged("CallSignalOnCurrentTagAndValues")]
    [ShowIf("isScenePath")]
    [LabelText("Scene")]
    SceneReference currentScene = default;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (isScenePath && !Application.isPlaying)
            startValue = startScene.ScenePath;
    }
#endif

    protected override void SetValue(string value)
    {
        base.SetValue(value);
        if (isScenePath)
        {
#if UNITY_EDITOR
            if (MustShowStartValue())
                SetSceneRef(ref startScene, value);
#endif
            SetSceneRef(ref currentScene, value);
        }
    }

    static void SetSceneRef(ref SceneReference scRef, string value)
    {
        if (scRef == null)
            scRef = new SceneReference();
        scRef.ScenePath = value;
    }

    [Command("set-string")]
    public static void SetString(StringSignal signal, string value) //Change type here
    {
        Set(signal, value);
        if (signal.isScenePath)
        {
#if UNITY_EDITOR
            if (signal.MustShowStartValue())
                SetSceneRef(ref signal.startScene, value);
#endif
            SetSceneRef(ref signal.currentScene, value);
        }
    }

    protected override string Calculate() //Change type here
    {
        return currentValue;
    }
}

#if UNITY_EDITOR
public class BaseSignalAttributeProcessor : OdinAttributeProcessor<StringSignal>
{
    public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if ((member.Name == "startValue") || (member.Name == "currentValue"))
        {
            attributes.Add(new TextAreaAttribute());
            attributes.Add(new DisableIfAttribute("isScenePath"));
        }
    }
}

[CustomPropertyDrawer(typeof(StringSignal))] //Change type here
public class StringSignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif

#if PLAYMAKER
namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Croxel")]
    [Tooltip("Calls a string signal")]
    public class PMCallStringSignal : FsmStateAction
    {
        [Tooltip("String to send")]
        public FsmString value;

        public StringSignal signal;

        // Code that runs on entering the state.
        public override void OnEnter()
        {
            signal.CallSignal(value.Value);
            Finish();
        }
    }
}
#endif