using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using Lowscope.Saving;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveSignals : BSaver
{
    [SerializeField]
    string substring = "";
    [SerializeField]
    bool autoSave = false;
    [SerializeField]
    BaseSignal[] signals = null;

    static bool lateSave = false;

    void Awake()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += PlayModeChanged;
#endif
        if (autoSave)
            foreach (BaseSignal s in signals)
                s.AddListener(TriggerSave);
    }

#if UNITY_EDITOR
    public void PlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            UpdateSignals();
    }
#endif

    [ShowIf("@HasSubstring()")]
    [Button]
    public void UpdateSignals()
    {
        if (HasSubstring())
        {
            List<BaseSignal> signalsTemp = new List<BaseSignal>();
            foreach (KeyValuePair<Type, List<BaseSignal>> pair in BaseSignal.activeSignals)
                foreach (BaseSignal s in pair.Value)
                    if ((s != null) && s.name.Contains(substring))
                        signalsTemp.Add(s);
            signals = signalsTemp.ToArray();
        }
    }

    bool HasSubstring()
    {
        return !substring.IsNullOrEmpty();
    }

    public void TriggerSave()
    {
        lateSave = true;
    }

    void LateUpdate()
    {
        if (lateSave)
        {
            SaveMaster.WriteActiveSaveToDisk();
            lateSave = false;
        }
    }

    public override void Load(string data)
    {
        base.Load(data);

        List<IValueSignal> signals = new List<IValueSignal>();
        foreach (BaseSignal signal in this.signals)
            if (signal.GetType().IsOrInheritsFrom(typeof(IValueSignal)))
            {
                IValueSignal valueSig = (IValueSignal)(object)signal;
                valueSig.Reset();
                signals.Add(valueSig);
            }

        foreach (string signal in data.Split(','))
        {
            string[] signalData = signal.Split('=');
            signals.First(x => x.name == signalData[0]).SetValueParse(signalData[1]);
        }
    }

    public override string Save()
    {
        Dictionary<string, string> signalValues = new Dictionary<string, string>();
        foreach (BaseSignal signal in this.signals)
            if (signal.GetType().IsOrInheritsFrom(typeof(IValueSignal)))
            {
                IValueSignal valueSig = (IValueSignal)(object)signal;
                signalValues.Add(valueSig.name, valueSig.GetStringValue());
            }

        string save = string.Join(",", signalValues.Select(kv => kv.Key + "=" + kv.Value)
            .ToArray());
        return save;
    }

    public override bool ShouldISave()
    {
        return true;
    }

    public override void ResetSaver()
    {
        base.ResetSaver();
        foreach (BaseSignal signal in signals)
            signal.Reset();
    }
}
