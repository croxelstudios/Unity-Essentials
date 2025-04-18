using System;
using UnityEngine;
using UnityEngine.Events;

public class Vector2IntSignalListener : BBaseSignalListener
{
    //[ChangeCheck("UpdateSignals")]
    public SignalAction[] signalActions;
    [SerializeField]
    int _multiplier = 1;
    public int multiplier
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

    public void LaunchActions()
    {
        for (int i = 0; i < signalActions.Length; i++)
            LaunchActions(i, signalActions[i].signal.currentValue);
    }

    public void LaunchActions(int index, Vector2Int value) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signalActions[index].actions?.Invoke(value * multiplier); //Change type here
    }

#if UNITY_EDITOR
    [ContextMenu("Create new Vector2IntSignal")] //Change type here
    void CreateSignal()
    {
        ScriptableObjectUtils.CreateScriptableObjectAsset<Vector2IntSignal>("Vector2IntSignal", "New Vector2IntSignal"); //Change type here (3)
    }
#endif

    [Serializable]
    public struct SignalAction
    {
        public string name;
        public Vector2IntSignal signal; //Change type here
        public Vector2IntEvent actions; //Change type here

        public SignalAction(Vector2IntSignal signal, Vector2IntEvent actions) //Change type here (2)
        {
            this.signal = signal;
            this.actions = actions;
            name = signal.name;
        }
    }

    [Serializable]
    public class Vector2IntEvent : UnityEvent<Vector2Int> { } //Change type here
}