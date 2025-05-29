using System;
using UnityEngine;
using UnityEngine.Events;

public class FloatSignalListener : BBaseSignalListener<float>
{
    //[ChangeCheck("UpdateSignals")]
    public SignalAction[] signalActions;
    [SerializeField]
    float _multiplier = 1f;
    public float multiplier
    {
        get { return _multiplier; }
        set
        {
            _multiplier = value;
            if (launchOnMultiplierChange)
                for (int i = 0; i < signalActions.Length; i++)
                    LaunchActions(i, signalActions[i].signal.currentValue); //Change type here
        }
    }
    [SerializeField]
    bool launchOnEnable = true;
    [SerializeField]
    bool launchOnMultiplierChange = true;

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

    public override void LaunchActions()
    {
        for (int i = 0; i < signalActions.Length; i++)
            LaunchActions(i, signalActions[i].signal.currentValue);
    }

    public override void LaunchActions(int index, float value) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signalActions[index].actions?.Invoke(value * multiplier); //Change type here
    }

#if UNITY_EDITOR
    [ContextMenu("Create new FloatSignal")] //Change type here
    void CreateSignal()
    {
        ScriptableObjectUtils.CreateScriptableObjectAsset<FloatSignal>("FloatSignal", "New FloatSignal"); //Change type here (3)
    }
#endif

    [Serializable]
    public struct SignalAction
    {
        public string name;
        public FloatSignal signal; //Change type here
        public DXFloatEvent actions; //Change type here

        public SignalAction(FloatSignal signal, DXFloatEvent actions) //Change type here (2)
        {
            this.signal = signal;
            this.actions = actions;
            name = signal.name;
        }
    }
}
