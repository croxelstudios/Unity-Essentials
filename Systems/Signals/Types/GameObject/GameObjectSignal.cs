using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/GameObjectSignal")] //Change type here
public class GameObjectSignal : BaseSignal //Change type here
{
    [SerializeField]
    bool resetValueOnStart = true;
    [SerializeField]
    [ShowIf("CanShowStartValue")]
    GameObject startValue = null; //Change type here
    [HideIf("CanShowStartValue")]
    [OnValueChanged("CallSignalOnCurrentTagAndValues")]
    public GameObject currentValue = null; //Change type here

    [TagSelector]
    public void CallSignal(GameObject value, string tag = "") //Change type here
    {
        if (value != currentValue)
        {
            currentValue = value;
            beforeCall?.Invoke();
            if (dynamicSearch) DynamicSearch<GameObjectSignalListener>(); //Change type here
            if (listeners != null)
            {
                GameObject finalValue = Calculate(); //Change type here
                for (int i = (listeners.Count - 1); i >= 0; i--)
                {
                    if ((tag == "") || (tag == listeners[i].receiver.tag))
                        ((GameObjectSignalListener)listeners[i].receiver). //Change type here
                            LaunchActions(listeners[i].index, finalValue);
                }
            }
            if (dynamicSearch) listeners = null;
            called?.Invoke();
        }
    }

    public void CallSignal(GameObject value) //Change type here
    {
        CallSignal(value, "");
    }

    [Command("set-gameobject")]
    public static void SetColor(GameObjectSignal signal, GameObject value) //Change type here
    {
        signal.CallSignal(value);
    }

    GameObject Calculate() //Change type here
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
[CustomPropertyDrawer(typeof(GameObjectSignal))] //Change type here
public class GameObjectSignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif
