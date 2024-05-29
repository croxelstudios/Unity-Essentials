using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/FloatSignal")] //Change type here
public class FloatSignal : BaseSignal //Change type here
{
    [SerializeField]
    bool resetValueOnStart = true;
    [SerializeField]
    bool callWithSameValue = false;
    [SerializeField]
    [ShowIf("CanShowStartValue")]
    float startValue = 0f; //Change type here
    [HideIf("CanShowStartValue")]
    [OnValueChanged("CallSignalOnCurrentTagAndValues")]
    public float currentValue = 0f; //Change type here

    [TagSelector]
    public void CallSignal(float value, string tag = "") //Change type here
    {
        if (callWithSameValue || value != currentValue)
        {
            currentValue = value;
            beforeCall?.Invoke();
            if (dynamicSearch) DynamicSearch<FloatSignalListener>(); //Change type here
            if (listeners != null)
            {
                float finalValue = Calculate(); //Change type here
                for (int i = (listeners.Count - 1); i >= 0; i--)
                {
                    if ((tag == "") || (tag == listeners[i].receiver.tag))
                        ((FloatSignalListener)listeners[i].receiver). //Change type here
                            LaunchActions(listeners[i].index, finalValue);
                }
            }
            if (dynamicSearch) listeners = null;
            called?.Invoke();
        }
    }

    public void CallSignal(float value) //Change type here
    {
        CallSignal(value, "");
    }

    [Command("set-float")]
    public static void SetFloat(FloatSignal signal, float value) //Change type here
    {
        signal.CallSignal(value);
    }

    float Calculate() //Change type here
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
[CustomPropertyDrawer(typeof(FloatSignal))] //Change type here
public class FloatSignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif

#if PLAYMAKER
namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Croxel")]
    [Tooltip("Calls a float signal")]
    public class PMCallFloatSignal : FsmStateAction
    {
        [Tooltip("Float to send")]
        public FsmFloat value;

        public FloatSignal signal;

        // Code that runs on entering the state.
        public override void OnEnter()
        {
            signal.CallSignal(value.Value);
            Finish();
        }
    }
}
#endif