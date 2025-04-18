using System;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class ColorSignalListener : BBaseSignalListener
{
    //[ChangeCheck("UpdateSignals")]
    public SignalAction[] signalActions;
    [SerializeField]
    Color multiplier = Color.white;
    [SerializeField]
    bool launchOnEnable = true;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (launchOnEnable) LaunchActions();
    }

    public override void UpdateSignals()
    {
        base.UpdateSignals();
        if (signals.Length != signalActions.Length)
            signals = new BaseSignal[signalActions.Length];
        for (int i = 0; i < signals.Length; i++)
            signals[i] = signalActions[i].signal;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (signalActions != null)
            for (int i = 0; i < signalActions.Length; i++)
                if ((signalActions[i].name == "") && (signalActions[i].signal != null))
                    signalActions[i].name = signalActions[i].signal.name;
    }
#endif

    public void LaunchActions()
    {
        for (int i = 0; i < signalActions.Length; i++)
            LaunchActions(i, signalActions[i].signal.currentValue);
    }

    public void LaunchActions(int index, Color color) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signalActions[index].actions?.Invoke(color * multiplier); //Change type here
    }

#if UNITY_EDITOR
    [ContextMenu("Create new ColorSignal")] //Change type here
    void CreateSignal()
    {
        ScriptableObjectUtils.CreateScriptableObjectAsset<ColorSignal>("ColorSignal", "New ColorSignal"); //Change type here (3)
    }
#endif

    [Serializable]
    public struct SignalAction
    {
        public string name;
        public ColorSignal signal; //Change type here
        public DXColorEvent actions; //Change type here

        public SignalAction(ColorSignal signal, DXColorEvent actions) //Change type here (2)
        {
            this.signal = signal;
            this.actions = actions;
            name = signal.name;
        }
    }
}
