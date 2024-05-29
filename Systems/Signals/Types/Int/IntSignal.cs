using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/IntSignal")] //Change type here
public class IntSignal : BaseSignal //Change type here
{
    [SerializeField]
    bool resetValueOnStart = true;
    [SerializeField]
    [ShowIf("CanShowStartValue")]
    int startValue = 0; //Change type here
    [HideIf("CanShowStartValue")]
    [OnValueChanged("CallSignalOnCurrentTagAndValues")]
    public int currentValue = 0; //Change type here

    [TagSelector]
    public void CallSignal(int value, string tag = "") //Change type here
    {
        if (value != currentValue)
        {
            currentValue = value;
            beforeCall?.Invoke();
            if (dynamicSearch) DynamicSearch<IntSignalListener>(); //Change type here
            if (listeners != null)
            {
                int finalValue = Calculate(); //Change type here
                for (int i = (listeners.Count - 1); i >= 0; i--)
                {
                    if ((tag == "") || (tag == listeners[i].receiver.tag))
                        ((IntSignalListener)listeners[i].receiver). //Change type here
                            LaunchActions(listeners[i].index, finalValue);
                }
            }
            if (dynamicSearch) listeners = null;
            called?.Invoke();
        }
    }
    //TO DO: Add and Subtract events. Repeat these n times when the value jumps?
    //Should be implemented in function above.

    public void CallSignal(int value) //Change type here
    {
        CallSignal(value, "");
    }

    [Command("set-int")]
    public static void SetInt(IntSignal signal, int value) //Change type here
    {
        signal.CallSignal(value);
    }

    public void Add(int amount, string tag = "")
    {
        CallSignal(currentValue + amount, tag);
    }

    public void Subtract(int amount, string tag = "")
    {
        Add(-amount, tag);
    }

    public void Add(int amount)
    {
        Add(amount, "");
    }

    public void Subtract(int amount)
    {
        Subtract(amount, "");
    }

    public void Add()
    {
        Add(1, "");
    }

    public void Subtract()
    {
        Subtract(1, "");
    }

    int Calculate() //Change type here
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
[CustomPropertyDrawer(typeof(IntSignal))] //Change type here
public class IntSignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif

#if PLAYMAKER
namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Croxel")]
    [Tooltip("Calls a int signal")]
    public class PMCallIntSignal : FsmStateAction
    {
        [Tooltip("Int to send")]
        public FsmInt value;

        public IntSignal signal;

        // Code that runs on entering the state.
        public override void OnEnter()
        {
            signal.CallSignal(value.Value);
            Finish();
        }
    }
}
#endif