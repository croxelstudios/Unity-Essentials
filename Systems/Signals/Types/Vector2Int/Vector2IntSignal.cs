using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/Vector2IntSignal")] //Change type here
public class Vector2IntSignal : BaseSignal //Change type here
{
    [SerializeField]
    bool resetValueOnStart = true;
    [SerializeField]
    [ShowIf("CanShowStartValue")]
    Vector2Int startValue = Vector2Int.zero; //Change type here
    [HideIf("CanShowStartValue")]
    [OnValueChanged("CallSignalOnCurrentTagAndValues")]
    public Vector2Int currentValue = Vector2Int.zero; //Change type here

    [TagSelector]
    public void CallSignal(Vector2Int value, string tag = "") //Change type here
    {
        if (value != currentValue)
        {
            currentValue = value;
            beforeCall?.Invoke();
            if (dynamicSearch) DynamicSearch<Vector2IntSignalListener>(); //Change type here
            if (listeners != null)
            {
                Vector2Int finalValue = Calculate(); //Change type here
                for (int i = (listeners.Count - 1); i >= 0; i--)
                {
                    if ((tag == "") || (tag == listeners[i].receiver.tag))
                        ((Vector2IntSignalListener)listeners[i].receiver). //Change type here
                            LaunchActions(listeners[i].index, finalValue);
                }
            }
            if (dynamicSearch) listeners = null;
            called?.Invoke();
        }
    }

    public void CallSignal(Vector2Int value) //Change type here
    {
        CallSignal(value, "");
    }

    [Command("set-vector2int")]
    public static void SetVector2Int(Vector2IntSignal signal, Vector2Int value) //Change type here
    {
        signal.CallSignal(value);
    }

    Vector2Int Calculate() //Change type here
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
[CustomPropertyDrawer(typeof(Vector2IntSignal))] //Change type here
public class Vector2IntSignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif
