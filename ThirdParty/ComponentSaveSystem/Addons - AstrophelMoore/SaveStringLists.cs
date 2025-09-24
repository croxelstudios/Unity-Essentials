using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using Lowscope.Saving;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveStringLists : BSaver
{
    [SerializeField]
    string substring = "";
    [SerializeField]
    bool autoSave = false;
    [SerializeField]
    StringList[] lists = null;

    private void Awake()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += PlayModeChanged;
#endif
        if (autoSave)
            foreach (StringList s in lists)
            {
                s.valueAdded = s.valueAdded.CreateAddListener<DXStringEvent, string>(TriggerSave);
                s.valueRemoved = s.valueRemoved.CreateAddListener<DXStringEvent, string>(TriggerSave);
            }
    }

#if UNITY_EDITOR
    public void PlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            UpdateLists();
    }
#endif

    [ShowIf("@HasSubstring()")]
    [Button]
    public void UpdateLists()
    {
        if (HasSubstring())
        {
            List<StringList> signalsTemp = new List<StringList>();
            foreach (StringList list in StringList.allLists)
                if (list.name.Contains(substring))
                    signalsTemp.Add(list);
            lists = signalsTemp.ToArray();
        }
    }

    bool HasSubstring()
    {
        return !substring.IsNullOrEmpty();
    }

    public void TriggerSave(string save)
    {
        SaveMaster.WriteActiveSaveToDisk();
    }

    public override void Load(string data)
    {
        base.Load(data);

        foreach (StringList list in lists)
            list.Reset();

        foreach (string list in data.Split(','))
        {
            string[] listData = list.Split('=');
            lists.First(x => x.name == listData[0]).AddValues(StringToStringList(listData[1]));
        }
    }

    public override string Save()
    {
        Dictionary<string, string> listsValues = new Dictionary<string, string>();
        foreach (StringList s in lists)
            listsValues.Add(s.name, StringListToString(s));

        string save = string.Join(",", listsValues.Select(kv => kv.Key + "=" + kv.Value)
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
        foreach (StringList list in lists)
            list.Reset();
    }

    string StringListToString(StringList list)
    {
        string result = "(";
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0) result += ";";
            result += list.GetValue(i);
        }
        result += ")";
        return result;
    }

    string[] StringToStringList(string list)
    {
        list = list.Trim(new char[] {'(', ')'});
        return list.Split(';');
    }
}
