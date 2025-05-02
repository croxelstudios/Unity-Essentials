using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class SignalSettings_Local : MonoBehaviour
{
    //TO DO: This should somehow launch the signals only in this object and then reset them.
    //Is that even possible?
    [SerializeField]
    [PropertyOrder(-1)]
    string substring = "";

    [DefaultDrawer]
    [SerializeField]
    BaseSignal[] signals = null;

    public SignalSettings.Holder holder = new SignalSettings.Holder();

#if UNITY_EDITOR
    void OnValidate()
    {
        holder = new SignalSettings.Holder(signals, substring);
    }

    [Button]
    [PropertyOrder(-1)]
    public void SearchSubstring()
    {
        if (!string.IsNullOrEmpty(substring))
        {
            List< BaseSignal> list = new List<BaseSignal>();
            list.AddRange(signals);
            for (int i = list.Count - 1; i >= 0; i--)
                if ((list[i] == null) || !list[i].name.Contains(substring))
                    list.RemoveAt(i);

            BaseSignal[] sig = BaseSignal.GetFromSubstring<BaseSignal>(substring);
            for (int i = 0; i < sig.Length; i++)
                if (!list.Contains(sig[i]))
                    list.Add(sig[i]);

            signals = list.ToArray();
            holder = new SignalSettings.Holder(signals, substring);
        }
    }
#endif
}
