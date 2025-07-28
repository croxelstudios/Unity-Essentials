using System;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public class IntSignalListener : BBaseSignalListener<int>
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
        if (launchOnEnable) LaunchActions(true);
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
        LaunchActions(false);
    }

    public void LaunchActions(bool justEnabled)
    {
        for (int i = 0; i < signalActions.Length; i++)
            LaunchActions(i, signalActions[i].signal.currentValue, justEnabled);
    }

    public override void LaunchActions(int index, int value) //Change type here
    {
        LaunchActions(index, value, false);
    }

    public void LaunchActions(int index, int value, bool justEnabled) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
        {
            signalActions[index].actions?.
                Invoke(Mathf.FloorToInt(value * multiplier)); //Change type here
            if (justEnabled)
                signalActions[index].onlyOnEnable.actions?.
                    Invoke(Mathf.FloorToInt(value * multiplier)); //Change type here
            else
                signalActions[index].onlyWhenAlreadyEnabled.actions?.
                    Invoke(Mathf.FloorToInt(value * multiplier)); //Change type here
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Create new IntSignal")] //Change type here
    void CreateSignal()
    {
        ScriptableObjectUtils.CreateScriptableObjectAsset<IntSignal>("IntSignal", "New IntSignal"); //Change type here (3)
    }
#endif

    [Serializable]
    public struct SignalAction
    {
        [FoldoutGroup("@name")]
        public string name;
        [FoldoutGroup("@name")]
        public IntSignal signal; //Change type here
        [FoldoutGroup("@name")]
        public DXIntEvent actions; //Change type here
        [FoldoutGroup("@name/$AlreadyEnabledFoldout")]
        public ExtraEvent onlyWhenAlreadyEnabled;
        [FoldoutGroup("@name/$OnEnableFoldout")]
        public ExtraEvent onlyOnEnable;

        public SignalAction(IntSignal signal, DXIntEvent actions) //Change type here (2)
        {
            this.signal = signal;
            this.actions = actions;
            name = signal.name;
            onlyWhenAlreadyEnabled = new ExtraEvent(null);
            onlyOnEnable = new ExtraEvent(null);
        }

#if UNITY_EDITOR
        string AlreadyEnabledFoldout()
        {
            return "Only When Already Enabled" + (onlyWhenAlreadyEnabled.IsNull() ? "" : " ⚠");
        }

        string OnEnableFoldout()
        {
            return "Only On Enable" + (onlyOnEnable.IsNull() ? "" : " ⚠");
        }
#endif
    }

    [HideLabel]
    [InlineProperty]
    [Serializable]
    public struct ExtraEvent
    {
        public DXIntEvent actions; //Change type here

        public ExtraEvent(DXIntEvent actions)
        {
            this.actions = actions;
        }

        public bool IsNull()
        {
            return actions.IsNull();
        }
    }
}
