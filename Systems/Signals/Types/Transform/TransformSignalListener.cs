using System;
using UnityEngine;

public class TransformSignalListener : BBaseSignalListener<Transform>
{
    //[ChangeCheck("UpdateSignals")]
    public SignalAction[] signalActions;

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

    public override void LaunchActions(int index, Transform transform) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signalActions[index].actions?.Invoke(transform); //Change type here
    }

#if UNITY_EDITOR
    [ContextMenu("Create new GameObjectSignal")] //Change type here
    void CreateSignal()
    {
        ScriptableObjectUtils.CreateScriptableObjectAsset<TransformSignal>("TransformSignal", "New TransformSignal"); //Change type here (3)
    }
#endif

    [Serializable]
    public struct SignalAction
    {
        public string name { get { return signal ? signal.name : "None"; } }
        public TransformSignal signal; //Change type here
        public DXTransformEvent actions; //Change type here

        public SignalAction(TransformSignal signal, DXTransformEvent actions) //Change type here (2)
        {
            this.signal = signal;
            this.actions = actions;
        }
    }
}
