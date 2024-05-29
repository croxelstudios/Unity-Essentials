using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/StringSignal")] //Change type here
public class StringSignal : BaseSignal //Change type here
{
    [SerializeField]
    bool resetValueOnStart = true;
    [SerializeField]
    [ShowIf("CanShowStartValue")]
    string startValue = ""; //Change type here
    [HideIf("CanShowStartValue")]
    [OnValueChanged("CallSignalOnCurrentTagAndValues")]
    public string currentValue = ""; //Change type here

    [TagSelector]
    public void CallSignal(string value, string tag = "") //Change type here
    {
        if (value != currentValue)
        {
            currentValue = value;
            beforeCall?.Invoke();
            if (dynamicSearch) DynamicSearch<StringSignalListener>(); //Change type here
            if (listeners != null)
            {
                string finalValue = Calculate(); //Change type here
                for (int i = (listeners.Count - 1); i >= 0; i--)
                {
                    if ((tag == "") || (tag == listeners[i].receiver.tag))
                        ((StringSignalListener)listeners[i].receiver). //Change type here
                            LaunchActions(listeners[i].index, finalValue);
                }
            }
            if (dynamicSearch) listeners = null;
            called?.Invoke();
        }
    }

    public void CallSignal(string value) //Change type here
    {
        CallSignal(value, "");
    }

    [Command("set-string")]
    public static void SetString(StringSignal signal, string value) //Change type here
    {
        signal.CallSignal(value);
    }

    string Calculate() //Change type here
    {
        return currentValue;
    }

#if UNITY_EDITOR
    public bool CanShowStartValue()
    {
        return resetValueOnStart && !Application.isPlaying;
    }

    public void CallSignalOnCurrentTagAndValues()
    {
        if (Application.isPlaying)
            CallSignal(currentValue, currentTag);
    }
#endif

    protected override void OnLoad()
    {
        base.OnLoad();
        Reset();
    }

    public override void Reset()
    {
        if (resetValueOnStart)
            currentValue = startValue;
    }
}

#if UNITY_EDITOR
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