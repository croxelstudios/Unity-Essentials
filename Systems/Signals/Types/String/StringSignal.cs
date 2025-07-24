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
    [SerializeField]
    [ShowIf("isScenePath")]
    SceneReference scene;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (isScenePath && !Application.isPlaying)
            startValue = scene.ScenePath;
    }
#endif

    protected override void SetValue(string value)
    {
        base.SetValue(value);
        if (isScenePath)
        {
            if (scene == null)
                scene = new SceneReference();
            scene.ScenePath = value;
        }
    }

    [Command("set-string")]
    public static void SetString(StringSignal signal, string value) //Change type here
    {
        Set(signal, value);
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