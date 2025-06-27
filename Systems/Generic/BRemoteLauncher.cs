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

        if (staticArray.NotNullContainsKey(t))
        {
            staticArray[t].ClearNulls();
            foreach (KeyValuePair<string, List<Component>> pair0 in staticArray[t])
                pair0.Value.ClearNulls();
        }
        else staticArray = staticArray.CreateAdd(t, new Dictionary<string, List<Component>>());

        if (staticArray[t].ContainsKey(filterByTag) &&
            ((searchMode == SearchMode.searchOnce) ||
            ((searchMode == SearchMode.searchWhenNull) && (staticArray[t][filterByTag].Count > 0))))
            array = ComponentToTypeArray<T>(staticArray[t][filterByTag].ToArray());
        else
        {
            if (filterByTag == "")
                staticArray[t] = staticArray[t].CreateAddRange(filterByTag,
                    FindObjectsByType<T>(FindObjectsSortMode.None));
            else staticArray[t] = staticArray[t].CreateAddRange(filterByTag,
                FilterByTag<T>(filterByTag));
            array = ComponentToTypeArray<T>(staticArray[t][filterByTag].ToArray());
        }

        if (extraTags != null)
        {
            for (int i = 0; i < extraTags.Length; i++)
            {
                if (staticArray[t].ContainsKey(extraTags[i]) &&
            ((searchMode == SearchMode.searchOnce) || ((searchMode == SearchMode.searchWhenNull) && (staticArray[t][filterByTag].Count > 0))))
                    array.Concat(ComponentToTypeArray<T>(staticArray[t][extraTags[i]].ToArray())).ToArray();
                else
                {
                    staticArray[t] = staticArray[t].CreateAddRange(extraTags[i],
                        FilterByTag<T>(extraTags[i]));
                    array.Concat(ComponentToTypeArray<T>(staticArray[t][extraTags[i]].ToArray())).ToArray();
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
