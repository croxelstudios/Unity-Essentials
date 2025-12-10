using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Croxel Scriptables/Collections/String List")]
public class StringList : ScriptableObject
{
    [SerializeField]
    bool resetValueOnStart = true;
    [SerializeField]
    [LabelText("Values")]
    [ShowIf("MustShowStartValue")]
    protected string[] tags = null;
    [SerializeField]
    [LabelText("Values")]
    [HideIf("MustShowStartValue")]
    [OnValueChanged("SendEventsOnArrayChange")]
    protected string[] runtimeValues = null;
    public DXStringEvent valueAdded = null;
    public DXStringEvent valueRemoved = null;
    public static List<StringList> allLists;
    public int Count
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return tags.Length;
            else
#endif
                return runtimeValues.Length;
        }
    }

    public virtual void AddValue(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && (!HasValue(value)))
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                tags = tags.Concat(new[] { value }).ToArray();
            else
#endif
                runtimeValues = runtimeValues.Concat(new[] { value }).ToArray();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            valueAdded?.Invoke(value);
        }
    }

    public virtual void AddValues(IEnumerable<string> values)
    {
        if (values != null)
            foreach (string value in values) AddValue(value);
    }

    public void RemoveValue(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && HasValue(value))
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                tags = tags.Where(t => t != value).ToArray();
            else
#endif
                runtimeValues = runtimeValues.Where(t => t != value).ToArray();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            valueRemoved?.Invoke(value);
        }
    }

    public bool HasValue(string value)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return tags.Contains(value);
        else
#endif
            return runtimeValues.Contains(value);
    }

    void OnEnable()
    {
        allLists = allLists.CreateAdd(this);
        if (resetValueOnStart)
            Reset();
    }

    void OnDisable()
    {
        allLists.SmartRemove(this);
    }

    public void Reset()
    {
        if (!runtimeValues.IsNullOrEmpty())
            foreach (string value in runtimeValues)
                if (!tags.Contains(value)) valueRemoved?.Invoke(value);

        if (!tags.IsNullOrEmpty())
        {
            foreach (string value in tags)
                if (runtimeValues.IsNullOrEmpty() || !runtimeValues.Contains(value))
                    valueAdded?.Invoke(value);

            runtimeValues = new string[tags.Length];
            for (int i = 0; i < tags.Length; i++)
                runtimeValues[i] = tags[i];
        }
    }

    public string GetValue(int index)
    {
        string[] values =
#if UNITY_EDITOR
            (!Application.isPlaying) ? tags :
#endif
            runtimeValues;
        if (index.IsBetween(0, values.Length))
            return values[index];
        else return null;
    }

    public string[] GetValues()
    {
        string[] values =
#if UNITY_EDITOR
            (!Application.isPlaying) ? tags :
#endif
            runtimeValues;
        return values;
    }

#if UNITY_EDITOR
    public bool MustShowStartValue()
    {
        return resetValueOnStart && !Application.isPlaying;
    }

    public void SendEventsOnArrayChange()
    {
        //TO DO
    }
#endif
}
