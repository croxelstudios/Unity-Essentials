using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using Lowscope.Saving;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveSignals : MonoBehaviour, ISaveable
{
    [SerializeField]
    string substring = "";
#if UNITY_EDITOR
    [SerializeField]
    bool autoSave = false;
#endif
    [SerializeField]
    BaseSignal[] signals = null;

#if UNITY_EDITOR
    private void Awake()
    {
        EditorApplication.playModeStateChanged += PlayModeChanged;

        if (autoSave)
            foreach (BaseSignal s in signals)
                s.AddListener(TriggerSave);
    }

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
        SaveMaster.WriteActiveSaveToDisk();
    }

    public void OnLoad(string data)
    {
        if (this.IsActiveAndEnabled())
        {
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
    }

    public string OnSave()
    {
        if (this.IsActiveAndEnabled())
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
        else return "";
    }

    public bool OnSaveCondition()
    {
        return true;
    }
}
