using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Object = UnityEngine.Object;

[HideLabel]
[InlineProperty]
[Serializable]
public struct LayerTagFilter : IObjectFilter, IEquatable<LayerTagFilter>
{
    [SerializeField]
    LayersFilter layersFilter;
    [SerializeField]
    TagsFilter tagsFilter;

    public static implicit operator LayersFilter(LayerTagFilter f) => f.layersFilter;

    public static implicit operator TagsFilter(LayerTagFilter f) => f.tagsFilter;

    public LayerTagFilter(LayerMask layers, bool whitelist_layers, string[] tags, bool whitelist_tags)
    {
        layersFilter = new LayersFilter(layers, whitelist_layers);
        tagsFilter = new TagsFilter(tags, whitelist_tags);
    }

    public LayerTagFilter(bool everyLayer)
    {
        layersFilter = new LayersFilter(everyLayer);
        tagsFilter = new TagsFilter(false);
    }

    public bool PassesTagsFilter<T>(T obj) where T : Object
    {
        return tagsFilter.PassesFilter(obj);
    }

    public bool PassesLayersFilter<T>(T obj) where T : Object
    {
        return layersFilter.PassesFilter(obj);
    }

    public bool PassesFilter<T>(T obj) where T : Object
    {
        GameObject go = obj.GameObj();
        return PassesTagsFilter(go) && PassesLayersFilter(go);
    }

    public T FindObject<T>() where T : Object
    {
        T[] objs = FindObjectsByTags<T>();
        return GetFirstFilteredByLayers(objs);
    }

    public T[] FindObjects<T>() where T : Object
    {
        T[] objs = FindObjectsByTags<T>();
        return GetFilteredByLayers(objs);
    }

    public T FindObjectInChildren<T>(GameObject parent) where T : Object
    {
        T[] objs = FindObjectsByTags<T>();
        return GetFirstFilteredByLayers(objs);
    }

    public T[] FindObjectsInChildren<T>(GameObject parent) where T : Object
    {
        T[] objs = FindObjectsByTags<T>();
        return GetFilteredByLayers(objs);
    }

    public T FindObjectByTags<T>() where T : Object
    {
        return tagsFilter.FindObject<T>();
    }

    public T[] FindObjectsByTags<T>() where T : Object
    {
        return tagsFilter.FindObjects<T>();
    }

    public T FindObjectByTagsInChildren<T>(GameObject parent) where T : Object
    {
        return tagsFilter.FindObjectInChildren<T>(parent);
    }

    public T[] FindObjectsByTagsInChildren<T>(GameObject parent) where T : Object
    {
        return tagsFilter.FindObjectsInChildren<T>(parent);
    }

    public T GetFirstFiltered<T>(IEnumerable<T> list) where T : Object
    {
        foreach (T obj in list)
            if (PassesFilter(obj))
                return obj;
        return null;
    }

    public T[] GetFiltered<T>(IEnumerable<T> list) where T : Object
    {
        List<T> filteredObjs = new List<T>();
        foreach (T obj in list)
            if (PassesFilter(obj))
                filteredObjs.Add(obj);

        return filteredObjs.ToArray();
    }

    public T GetFirstFilteredByTags<T>(IEnumerable<T> list) where T : Object
    {
        return tagsFilter.GetFirstFiltered(list);
    }

    public T[] GetFilteredByTags<T>(IEnumerable<T> list) where T : Object
    {
        return tagsFilter.GetFiltered(list);
    }

    public T GetFirstFilteredByLayers<T>(IEnumerable<T> list) where T : Object
    {
        return layersFilter.GetFirstFiltered(list);
    }

    public T[] GetFilteredByLayers<T>(IEnumerable<T> list) where T : Object
    {
        return layersFilter.GetFiltered(list);
    }

    // ------------------------------------------------------------------------------

    public override bool Equals(object other)
    {
        if (!(other is LayerTagFilter)) return false;
        return Equals((LayerTagFilter)other);
    }

    public bool Equals(LayerTagFilter other)
    {
        return (layersFilter == other.layersFilter)
            && (tagsFilter == other.tagsFilter);
    }

    public override int GetHashCode()
    {
        return HashMaker.Elements(layersFilter, tagsFilter);
    }

    public static bool operator ==(LayerTagFilter o1, LayerTagFilter o2)
    {
        return o1.Equals(o2);
    }

    public static bool operator !=(LayerTagFilter o1, LayerTagFilter o2)
    {
        return !o1.Equals(o2);
    }
}

[HideLabel]
[InlineProperty]
[Serializable]
public struct LayersFilter : IObjectFilter, IEquatable<LayersFilter>
{
    [SerializeField]
    [HorizontalGroup("Layer")]
    LayerMask layers;
    [HideLabel]
    [SerializeField]
    [ToggleButtons("Is Whitelist", "Is Blacklist", true)]
    [HorizontalGroup("Layer", Width = 80f)]
    bool whitelist;

    public LayersFilter(LayerMask layers, bool whitelist)
    {
        this.layers = layers;
        this.whitelist = whitelist;
    }

    public LayersFilter(bool everyLayer)
    {
        layers = everyLayer ? ~0 : 0;
        whitelist = everyLayer ? true : false;
    }

    public bool PassesFilter<T>(T obj) where T : Object
    {
        GameObject go = obj.GameObj();
        return !(whitelist ^ layers.ContainsLayer(go.layer));
    }

    public T GetFirstFiltered<T>(IEnumerable<T> list) where T : Object
    {
        foreach (T obj in list)
            if (PassesFilter(obj))
                return obj;
        return null;
    }

    public T[] GetFiltered<T>(IEnumerable<T> list) where T : Object
    {
        List<T> filteredObjs = new List<T>();
        foreach (T obj in list)
            if (PassesFilter(obj))
                filteredObjs.Add(obj);

        return filteredObjs.ToArray();
    }

    // ------------------------------------------------------------------------------

    public override bool Equals(object other)
    {
        if (!(other is LayersFilter)) return false;
        return Equals((LayersFilter)other);
    }

    public bool Equals(LayersFilter other)
    {
        return (layers == other.layers)
            && (whitelist == other.whitelist);
    }

    public override int GetHashCode()
    {
        return HashMaker.Elements(layers, whitelist);
    }

    public static bool operator ==(LayersFilter o1, LayersFilter o2)
    {
        return o1.Equals(o2);
    }

    public static bool operator !=(LayersFilter o1, LayersFilter o2)
    {
        return !o1.Equals(o2);
    }
}

[HideLabel]
[InlineProperty]
[Serializable]
public struct TagsFilter : IObjectFilter, IEquatable<TagsFilter>
{
    [TagSelector]
    [SerializeField]
    [HorizontalGroup("Tags")]
    string[] tags;
    [HideLabel]
    [SerializeField]
    [ToggleButtons("Is Whitelist", "Is Blacklist", true)]
    [HorizontalGroup("Tags", Width = 80f)]
    bool whitelist;

    public TagsFilter(string[] tags, bool whitelist)
    {
        this.tags = tags;
        this.whitelist = whitelist;
    }

    public TagsFilter(bool whitelist)
    {
        tags = new string[0];
        this.whitelist = whitelist;
    }

    public bool PassesFilter<T>(T obj) where T : Object
    {
        GameObject go = obj.GameObj();
        return !(whitelist ^ tags.Contains(go.tag));
    }

    public T GetFirstFiltered<T>(IEnumerable<T> list) where T : Object
    {
        foreach (T obj in list)
            if (PassesFilter(obj))
                return obj;
        return null;
    }

    public T[] GetFiltered<T>(IEnumerable<T> list) where T : Object
    {
        List<T> filteredObjs = new List<T>();
        foreach (T obj in list)
            if (PassesFilter(obj))
                filteredObjs.Add(obj);

        return filteredObjs.ToArray();
    }

    public T FindObject<T>() where T : Object
    {
        T[] objs;
        if (whitelist)
            return FindWithTag.Any<T>(tags);
        else
        {
            objs = Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            return GetFirstFiltered(objs);
        }
    }

    public T[] FindObjects<T>() where T : Object
    {
        T[] objs;
        if (whitelist)
            objs = FindWithTag.Anys<T>(tags);
        else
        {
            objs = Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            objs = GetFiltered(objs);
        }

        return objs;
    }

    public T FindObjectInChildren<T>(GameObject parent) where T : Object
    {
        T[] objs;
        if (whitelist)
            return FindWithTag.AnyInChildren<T>(parent, tags);
        else
        {
            if (typeof(T) == typeof(GameObject))
            {
                Transform[] trs = parent.GetComponentsInChildren<Transform>();
                objs = trs.Select(t => t.gameObject).OfType<T>().ToArray();
            }
            else objs = parent.GetComponentsInChildren<T>();
            return GetFirstFiltered(objs);
        }
    }

    public T[] FindObjectsInChildren<T>(GameObject parent) where T : Object
    {
        T[] objs;
        if (whitelist)
            return FindWithTag.AnysInChildren<T>(parent, tags);
        else
        {
            if (typeof(T) == typeof(GameObject))
            {
                Transform[] trs = parent.GetComponentsInChildren<Transform>();
                objs = trs.Select(t => t.gameObject).OfType<T>().ToArray();
            }
            else objs = parent.GetComponentsInChildren<T>();
            return GetFiltered(objs);
        }
    }

    // ------------------------------------------------------------------------------

    public override bool Equals(object other)
    {
        if (!(other is TagsFilter)) return false;
        return Equals((TagsFilter)other);
    }

    public bool Equals(TagsFilter other)
    {
        return (tags == other.tags)
            && (whitelist == other.whitelist);
    }

    public override int GetHashCode()
    {
        return HashMaker.Elements(tags, whitelist);
    }

    public static bool operator ==(TagsFilter o1, TagsFilter o2)
    {
        return o1.Equals(o2);
    }

    public static bool operator !=(TagsFilter o1, TagsFilter o2)
    {
        return !o1.Equals(o2);
    }
}

public interface IObjectFilter
{
    public bool PassesFilter<T>(T obj) where T : Object;

    public T GetFirstFiltered<T>(IEnumerable<T> list) where T : Object;

    public T[] GetFiltered<T>(IEnumerable<T> list) where T : Object;
}

