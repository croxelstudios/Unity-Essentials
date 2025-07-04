using Sirenix.OdinInspector;
using System;
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
        Reset();
    }

    public void Reset()
    {
        if (resetValueOnStart)
        {
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
