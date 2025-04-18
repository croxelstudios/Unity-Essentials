using System;
using UnityEngine;
using UnityEngine.Events;

public class Vector3SignalListener : BBaseSignalListener
{
    //[ChangeCheck("UpdateSignals")]
    public SignalAction[] signalActions;
    [SerializeField]
    float _multiplier = 1;
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

    public void LaunchActions()
    {
        for (int i = 0; i < signalActions.Length; i++)
            LaunchActions(i, signalActions[i].signal.currentValue);
    }

    public void LaunchActions(int index, Vector3 value) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signalActions[index].actions?.Invoke(value * multiplier); //Change type here
    }

#if UNITY_EDITOR
    [ContextMenu("Create new Vector3Signal")] //Change type here
    void CreateSignal()
    {
        ScriptableObjectUtils.CreateScriptableObjectAsset<Vector3Signal>("Vector2IntSignal", "New Vector2IntSignal"); //Change type here (3)
    }
#endif

    [Serializable]
    public struct SignalAction
    {
        public string name;
        public Vector3Signal signal; //Change type here
        public DXVectorEvent actions; //Change type here

        public SignalAction(Vector3Signal signal, DXVectorEvent actions) //Change type here (2)
        {
            this.signal = signal;
            this.actions = actions;
            name = signal.name;
        }
    }
}