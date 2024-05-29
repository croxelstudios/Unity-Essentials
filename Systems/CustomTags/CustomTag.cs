using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CustomTag : MonoBehaviour
{
    //[SerializeField]
    //bool _enabled = true;
    //public new bool enabled { get { return _enabled; } set { _enabled = value; } }
    public StringList tagList;
    [StringPopup(new string[] { "tagList", "tags" })]
    public int customTag;

    public static Dictionary<StringList, Dictionary<int, List<CustomTag>>> activeTagged { get; private set; }
    public GameObject[] includedGameObjects { get; private set; }

    void OnEnable()
    {
        if (activeTagged == null) activeTagged = new Dictionary<StringList, Dictionary<int, List<CustomTag>>>();
        if (!activeTagged.ContainsKey(tagList)) activeTagged.Add(tagList, new Dictionary<int, List<CustomTag>>());
        if (!activeTagged[tagList].ContainsKey(customTag)) activeTagged[tagList].Add(customTag, new List<CustomTag>());
        if (!activeTagged[tagList][customTag].Contains(this)) activeTagged[tagList][customTag].Add(this);

        includedGameObjects = GetChildren(transform).ToArray();
    }

    void OnDisable()
    {
        if ((activeTagged != null) &&
            (activeTagged.ContainsKey(tagList)) &&
            (activeTagged[tagList].ContainsKey(customTag)) &&
            (activeTagged[tagList][customTag].Contains(this)))
            activeTagged[tagList][customTag].Remove(this);
    }

    List<GameObject> GetChildren(Transform tr)
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < tr.childCount; i++)
            list.AddRange(GetChildren(tr.GetChild(i)));
        GameObject obj = tr.gameObject;
        if (!list.Contains(obj)) list.Add(obj);
        return list;
    }

    //[SerializeField]
    //string newCustomTag;

    //public void AddTag()
    //{
    //    if(tagList)
    //        tagList.AddTag(ref newCustomTag);
    //}

    //public bool Exists()
    //{
    //    if (tagList)
    //        return tagList.TagExists(newCustomTag);
    //    else
    //        return false;
    //}

    [StringPopup(new string[] { "tagList", "tags" })]
    public virtual void SwitchTag(int newTag)
    {
        customTag = newTag;
    }
}

public static class CustomTagExtension_Contains
{
    public static bool Contains(this List<CustomTag> list, GameObject obj)
    {
        foreach (CustomTag ct in list)
            if (ct.includedGameObjects.Contains(obj)) return true;
        return false;
    }
}

[Serializable]
[Sirenix.OdinInspector.InlineProperty]
[Sirenix.OdinInspector.HideLabel]
public struct CustomTagItem
{
    public StringList tagList;
    [StringPopup(new string[] { "tagList", "tags" })]
    public int customTag;

    //public string newCustomTag;

    //[Button]
    //public void AddTag()
    //{
    //    if (tagList && !Exists())
    //        tagList.AddTag(ref newCustomTag);
    //}

    //public bool Exists()
    //{

    //    if (tagList)
    //        return tagList.TagExists(newCustomTag);
    //    else
    //        return false;
    //}

    public bool Check(GameObject other)
    {
        if (tagList == null) return true;

        if (CustomTag.activeTagged.ContainsKey(tagList) &&
            CustomTag.activeTagged[tagList].ContainsKey(customTag) &&
            CustomTag.activeTagged[tagList][customTag].Contains(other))
            return true;
        else return false;
    }

    public bool CheckDirty(GameObject other, bool includeInactive = false)
    {
        if (tagList == null) return true;

        bool hasTag = false;
        CustomTag[] tagComponents = other.GetComponentsInParent<CustomTag>(includeInactive);
        foreach (CustomTag tagComponent in tagComponents)
        {
            if ((tagComponent.tagList == tagList) &&
                (customTag == tagComponent.customTag))
            {
                hasTag = true;
                break;
            }
        }

        return hasTag;
    }

    [StringPopup(new string[] { "tagList", "tags" })]
    public void SetCustomTag(int id)
    {
        customTag = id;
    }
}

[Serializable]
[Sirenix.OdinInspector.InlineProperty]
[Sirenix.OdinInspector.HideLabel]
public struct CustomTagItems
{
    public StringList tagList;
    [StringPopup(new string[] { "tagList", "tags" })]
    public int[] customTags;

    //public string newCustomTag;

    //[Button]
    //public void AddTag()
    //{
    //    if (tagList && !Exists())
    //        tagList.AddTag(ref newCustomTag);
    //}

    //public bool Exists()
    //{

    //    if (tagList)
    //        return tagList.TagExists(newCustomTag);
    //    else
    //        return false;
    //}

    public bool Check(GameObject other)
    {
        if (tagList == null) return true;

        bool hasTag = false;
        foreach (int tag in customTags)
        {
            if (CustomTag.activeTagged.ContainsKey(tagList) &&
                CustomTag.activeTagged[tagList].ContainsKey(tag) &&
                CustomTag.activeTagged[tagList][tag].Contains(other))
            {
                hasTag = true;
                break;
            }
        }

        return hasTag;
    }

    public bool CheckDirty(GameObject other)
    {
        if (tagList == null) return true;

        bool hasTag = false;
        CustomTag[] tagComponents = other.GetComponentsInParent<CustomTag>(true);
        foreach (CustomTag tagComponent in tagComponents)
        {
            if (tagComponent.enabled && (tagComponent.tagList == tagList) &&
                customTags.Contains(tagComponent.customTag))
            {
                hasTag = true;
                break;
            }
        }

        return hasTag;
    }

    [StringPopup(new string[] { "tagList", "tags" })]
    public void SetFirstCustomTag(int id)
    {
        if ((customTags == null) || (customTags.Length <= 0))
            customTags = new int[] { id };
        else customTags[0] = id;
    }
}
