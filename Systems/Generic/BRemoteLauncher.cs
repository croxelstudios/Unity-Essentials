using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BRemoteLauncher : MonoBehaviour
{
    [SerializeField]
    [Header("Optional:")]
    [TagSelector]
    string filterByTag = "";
    [SerializeField]
    [Tooltip("Won't work if there is no main tag")]
    [TagSelector]
    string[] extraTags = null;
    [Space]
    [SerializeField]
    protected SearchMode searchMode = SearchMode.searchWhenNull;

    protected enum SearchMode { searchWhenNull, searchOnce, searchEveryTime }
    public static Dictionary<Type, Dictionary<string, List<Component>>> staticArray;

    void OnDisable()
    {
    }

    protected void FillArrayAwake<T>(ref T[] array) where T : Component
    {
        if (searchMode != SearchMode.searchEveryTime) FillArray(ref array);
    }

    protected void FillArrayUpdate<T>(ref T[] array) where T : Component
    {
        array = array.ClearNulls();
        if (searchMode == SearchMode.searchEveryTime ||
            ((searchMode == SearchMode.searchWhenNull) && array.IsNullOrEmpty()))
            FillArray(ref array);
    }

    protected void FillArray<T>(ref T[] array) where T : Component
    {
        Type t = typeof(T);

        Dictionary<string, List<Component>> dict;
        if (staticArray.SmartGetValue(t, out dict))
        {
            dict = dict.ClearNulls();
            foreach (KeyValuePair<string, List<Component>> pair0 in dict)
                pair0.Value.ClearNulls();
        }
        else
        {
            dict = new Dictionary<string, List<Component>>();
            staticArray = staticArray.CreateAdd(t, dict);
        }

        List<Component> list;
        if (dict.SmartGetValue(filterByTag, out list) &&
            ((searchMode == SearchMode.searchOnce) ||
            ((searchMode == SearchMode.searchWhenNull) && (list.Count > 0))))
            array = ComponentToTypeArray<T>(list.ToArray());
        else
        {
            if (filterByTag == "")
                list = list.CreateAddRange(FindObjectsByType<T>(FindObjectsSortMode.None));
            else list = list.CreateAddRange(FilterByTag<T>(filterByTag));
            staticArray[t] = dict = dict.CreateAddRange(filterByTag, list);
            array = ComponentToTypeArray<T>(list.ToArray());
        }

        if (extraTags != null)
        {
            for (int i = 0; i < extraTags.Length; i++)
            {
                string extraTag = extraTags[i];
                if (dict.SmartGetValue(extraTag, out list) &&
                    ((searchMode == SearchMode.searchOnce) ||
                    ((searchMode == SearchMode.searchWhenNull) && (list.Count > 0))))
                    array.Concat(ComponentToTypeArray<T>(list.ToArray())).ToArray();
                else
                {
                    list = list.CreateAddRange(FilterByTag<T>(extraTag));
                    staticArray[t] = dict = dict.CreateAddRange(extraTag, list);
                    array.Concat(ComponentToTypeArray<T>(list.ToArray())).ToArray();
                }
            }
        }
    }

    protected T[] FilterByTag<T>(string tag) where T : Component
    {
        List<T> comp = new List<T>();
        comp.AddRange(FindObjectsByType<T>(FindObjectsSortMode.None));
        for (int i = comp.Count - 1; i > -1; i--)
            if (comp[i].tag != tag) comp.RemoveAt(i);
        return comp.ToArray();
    }

    protected T[] FilterByTags<T>(string[] tags) where T : Component
    {
        List<T> comp = new List<T>();
        comp.AddRange(FindObjectsByType<T>(FindObjectsSortMode.None));
        for (int i = comp.Count - 1; i > -1; i--)
            if (!tags.Contains(comp[i].tag)) comp.RemoveAt(i);
        return comp.ToArray();
    }

    T[] ComponentToTypeArray<T>(Component[] origin) where T : Component
    {
        T[] r = new T[origin.Length];
        for (int i = 0; i < r.Length; i++)
            r[i] = (T)origin[i];
        return r;
    }
}
