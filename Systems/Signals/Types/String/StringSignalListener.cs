using System;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class StringSignalListener : BBaseSignalListener<string>
{
    //[ChangeCheck("UpdateSignals")]
    public SignalAction[] signalActions;
    [SerializeField]
    bool launchOnEnable = true;
    //[SerializeField]
    //bool launchOnMultiplierChange = true;

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

    public override void LaunchActions()
    {
        for (int i = 0; i < signalActions.Length; i++)
            LaunchActions(i, signalActions[i].signal.currentValue);
    }

    public override void LaunchActions(int index, string value) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signalActions[index].actions?.Invoke(value); //Change type here
    }

#if UNITY_EDITOR
    [ContextMenu("Create new StringSignal")] //Change type here
    void CreateSignal()
    {
        ScriptableObjectUtils.CreateScriptableObjectAsset<StringSignal>("StringSignal", "New StringSignal"); //Change type here (3)
    }
#endif

    [Serializable]
    public struct SignalAction
    {
        public string name { get { return signal ? signal.name : "None"; } }
        public StringSignal signal; //Change type here
        public DXStringEvent actions; //Change type here

        public SignalAction(StringSignal signal, DXStringEvent actions) //Change type here (2)
        {
            this.signal = signal;
            this.actions = actions;
        }
    }
}
