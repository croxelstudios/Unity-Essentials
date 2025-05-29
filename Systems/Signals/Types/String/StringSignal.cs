using UnityEngine;
using QFSW.QC;
#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/StringSignal")] //Change type here
public class StringSignal : BaseSignal<string, StringSignalListener> //Change type here
{
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
            attributes.Add(new TextAreaAttribute());
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