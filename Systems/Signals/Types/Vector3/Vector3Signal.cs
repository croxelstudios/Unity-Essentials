using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/Vector3Signal")] //Change type here
public class Vector3Signal : BaseSignal //Change type here
{
    [SerializeField]
    bool resetValueOnStart = true;
    [SerializeField]
    bool callWithSameValue = false;
    [SerializeField]
    [ShowIf("CanShowStartValue")]
    Vector3 startValue = Vector3.zero; //Change type here
    [HideIf("CanShowStartValue")]
    [OnValueChanged("CallSignalOnCurrentTagAndValues")]
    public Vector3 currentValue = Vector3.zero; //Change type here

    [TagSelector]
    public void CallSignal(Vector3 value, string tag = "") //Change type here
    {
        if (callWithSameValue || value != currentValue)
        {
            currentValue = value;
            beforeCall?.Invoke();
            if (dynamicSearch) DynamicSearch<Vector3SignalListener>(); //Change type here
            if (listeners != null)
            {
                Vector3 finalValue = Calculate(); //Change type here
                for (int i = (listeners.Count - 1); i >= 0; i--)
                {
                    if ((tag == "") || (tag == listeners[i].receiver.tag))
                        ((Vector3SignalListener)listeners[i].receiver). //Change type here
                            LaunchActions(listeners[i].index, finalValue);
                }
            }
            if (dynamicSearch) listeners = null;
            called?.Invoke();
        }
    }

    public void CallSignal(Vector2 value, string tag = "")
    {
        CallSignal((Vector3)value, tag);
    }

    public void CallSignal(Vector3 value) //Change type here
    {
        CallSignal(value, "");
    }

    public void CallSignal(Vector2 value) //Change type here
    {
        CallSignal(value, "");
    }

    [Command("set-vector3")]
    public static void SetVector3(Vector3Signal signal, Vector3 value) //Change type here
    {
        signal.CallSignal(value);
    }

    Vector3 Calculate() //Change type here
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
[CustomPropertyDrawer(typeof(Vector3Signal))] //Change type here
public class Vector3SignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif
