using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class FlagChecker : BBaseSignalListener<bool>
{
    //[ChangeCheck("UpdateSignals")]
    public FlagAction[] flags;
    [SerializeField]
    bool launchOnEnable = true;
    public bool launchOnEnable_ { get { return launchOnEnable; } set { value = launchOnEnable; } }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (launchOnEnable) CheckFlags(true);
    }

    public override void UpdateSignals()
    {
        base.UpdateSignals();
        if (signals.Length != flags.Length)
            signals = new BaseSignal[flags.Length];
        for (int i = 0; i < signals.Length; i++)
            signals[i] = flags[i].flag;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (flags != null)
            for (int i = 0; i < flags.Length; i++)
                if ((flags[i].name == "") && (flags[i].flag != null))
                    flags[i].name = flags[i].flag.name;
    }
#endif

    public void CheckFlags()
    {
        CheckFlags(false);
    }

    public void CheckFlags(bool justEnabled)
    {
        for (int i = 0; i < flags.Length; i++)
            LaunchActions(i, flags[i].flag.currentValue, justEnabled);
    }

    public override void LaunchActions()
    {
        CheckFlags();
    }

    public override void LaunchActions(int index, bool value)
    {
        LaunchActions(index, value, false);
    }

    public void LaunchActions(int index, bool value, bool justEnabled) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
        {
            if (value) flags[index].whenTrue?.Invoke(); //Change type here
            else flags[index].whenFalse?.Invoke(); //Change type here

            if (justEnabled)
            {
                if (value) flags[index].onlyOnEnable.whenTrue?.Invoke(); //Change type here
                else flags[index].onlyOnEnable.whenFalse?.Invoke(); //Change type here
            }
            else
            {
                if (value) flags[index].onlyWhenAlreadyEnabled.whenTrue?.Invoke(); //Change type here
                else flags[index].onlyWhenAlreadyEnabled.whenFalse?.Invoke(); //Change type here
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Create new Flag")] //Change type here
    void CreateFlag()
    {
        ScriptableObjectUtils.CreateScriptableObjectAsset<Flag>("Flag", "New Flag"); //Change type here (3)
    }
#endif

    public void SetToTrue()
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            for (int i = 0; i < flags.Length; i++)
                SetToTrue(i);
    }

    public void SetToTrue(int index)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flags[index].flag.SetFlag(true);
    }

    public void SetToFalse()
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            for (int i = 0; i < flags.Length; i++)
                SetToFalse(i);
    }

    public void SetToFalse(int index)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flags[index].flag.SetFlag(false);
    }

    public void SwitchValue()
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            for (int i = 0; i < flags.Length; i++)
                SwitchValue(i);
    }

    public void SwitchValue(int index)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flags[index].flag.SwitchValue();
    }

    public void LaunchOnTrue()
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            for (int i = 0; i < flags.Length; i++)
                LaunchOnTrue(i);
    }

    [TagSelector]
    public void LaunchOnTrue(string tag)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            for (int i = 0; i < flags.Length; i++)
                LaunchOnTrue(i, tag);
    }

    public void LaunchOnTrue(GameObject obj)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            for (int i = 0; i < flags.Length; i++)
                LaunchOnTrue(i, obj);
    }

    public void LaunchOnTrue(int index)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flags[index].flag.LaunchOnTrue();
    }

    public void LaunchOnTrue(int index, string tag)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flags[index].flag.LaunchOnTrue(tag);
    }

    public void LaunchOnTrue(int index, GameObject obj)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flags[index].flag.LaunchOnTrue(obj);
    }

    public void LaunchOnFalse()
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            for (int i = 0; i < flags.Length; i++)
                LaunchOnFalse(i);
    }

    [TagSelector]
    public void LaunchOnFalse(string tag)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            for (int i = 0; i < flags.Length; i++)
                LaunchOnFalse(i, tag);
    }

    public void LaunchOnFalse(int index)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flags[index].flag.LaunchOnFalse();
    }

    public void LaunchOnFalse(GameObject obj)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            for (int i = 0; i < flags.Length; i++)
                LaunchOnFalse(i, obj);
    }

    public void LaunchOnFalse(int index, string tag)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flags[index].flag.LaunchOnFalse(tag);
    }

    public void LaunchOnFalse(int index, GameObject obj)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flags[index].flag.LaunchOnFalse(obj);
    }

    [Serializable]
    public struct FlagAction
    {
        [FoldoutGroup("@name")]
        public string name;
        [FoldoutGroup("@name")]
        public Flag flag; //Change type here
        [FoldoutGroup("@name")]
        public DXEvent whenTrue; //Change type here
        [FoldoutGroup("@name")]
        public DXEvent whenFalse; //Change type here
        [FoldoutGroup("@name/$AlreadyEnabledFoldout")]
        public ExtraEvents onlyWhenAlreadyEnabled;
        [FoldoutGroup("@name/$OnEnableFoldout")]
        public ExtraEvents onlyOnEnable;

        public FlagAction(Flag signal) //Change type here (2)
        {
            this.flag = signal;
            whenTrue = null;
            whenFalse = null;
            name = signal.name;
            onlyWhenAlreadyEnabled = new ExtraEvents(null, null);
            onlyOnEnable = new ExtraEvents(null, null);
        }

#if UNITY_EDITOR
        public string AlreadyEnabledFoldout()
        {
            return "Only When Already Enabled" + (onlyWhenAlreadyEnabled.IsNull() ? "" : " ⚠");
        }

        public string OnEnableFoldout()
        {
            return "Only On Enable" + (onlyOnEnable.IsNull() ? "" : " ⚠");
        }
#endif
    }

    [HideLabel]
    [InlineProperty]
    [Serializable]
    public struct ExtraEvents
    {
        public DXEvent whenTrue; //Change type here
        public DXEvent whenFalse; //Change type here

        public ExtraEvents(DXEvent whenTrue, DXEvent whenFalse)
        {
            this.whenTrue = whenTrue;
            this.whenFalse = whenFalse;
        }

        public bool IsNull()
        {
            return whenTrue.IsNull() && whenFalse.IsNull();
        }
    }
}
