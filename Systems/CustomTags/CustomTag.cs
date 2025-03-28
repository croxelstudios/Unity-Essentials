using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-9999)]
public class CustomTag : MonoBehaviour
{
    //[SerializeField]
    //bool _enabled = true;
    //public new bool enabled { get { return _enabled; } set { _enabled = value; } }
    [OnValueChanged("TagUpdateAction")]
    public CustomTagItem item;
    public DXIntEvent tagWasChanged = null;

    #region Statics
    public static Dictionary<StringList, Dictionary<int, List<CustomTag>>> activeTagged { get; private set; }
    
    public static void AddActiveTaggedObj(CustomTagItem item, CustomTag customTag)
    {
        if (activeTagged == null)
            activeTagged = new Dictionary<StringList, Dictionary<int, List<CustomTag>>>();
        if (!activeTagged.ContainsKey(item.tagList))
            activeTagged.Add(item.tagList, new Dictionary<int, List<CustomTag>>());
        if (!activeTagged[item.tagList].ContainsKey(item.customTag))
            activeTagged[item.tagList].Add(item.customTag, new List<CustomTag>());
        if (!activeTagged[item.tagList][item.customTag].Contains(customTag))
            activeTagged[item.tagList][item.customTag].Add(customTag);
    }

    public static void AddActiveTaggedObj(CustomTag customTag)
    {
        AddActiveTaggedObj(customTag.item, customTag);
    }

    public static void RemoveActiveTaggedObj(CustomTagItem item, CustomTag customTag)
    {
        if (IsItemActiveTagged(item) &&
            activeTagged[item.tagList][item.customTag].Contains(customTag))
            activeTagged[item.tagList][item.customTag].Remove(customTag);
    }

    public static void RemoveActiveTaggedObj(CustomTag customTag)
    {
        RemoveActiveTaggedObj(customTag.item, customTag);
    }

    public static bool IsItemActiveTagged(CustomTagItem item)
    {
        return (activeTagged != null) &&
            activeTagged.ContainsKey(item.tagList) &&
            activeTagged[item.tagList].ContainsKey(item.customTag);
    }

    public static List<CustomTag> GetActiveTagged(CustomTagItem item)
    {
        if (!IsItemActiveTagged(item)) return null;
        else return activeTagged[item.tagList][item.customTag];
    }
    #endregion

    bool AmIActiveTagged(CustomTagItem item)
    {
        return IsItemActiveTagged(item) &&
            activeTagged[item.tagList][item.customTag].Contains(this);
    }

    public bool AmIActiveTagged()
    {
        return AmIActiveTagged(item);
    }

    public GameObject[] includedGameObjects { get; private set; }

    void OnEnable()
    {
        AddActiveTaggedObj(this);
        includedGameObjects = GetChildren(transform).ToArray();
    }

    void OnDisable()
    {
        RemoveActiveTaggedObj(this);
    }

    public void TagUpdateAction()
    {
        tagWasChanged?.Invoke(item.customTag);
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

    [StringPopup(new string[] { "item", "tagList", "tags" })]
    public virtual void SwitchTag(int newTag)
    {
        item.customTag = newTag;
        TagUpdateAction();
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
public struct CustomTagItem : IEquatable<CustomTagItem>
{
    public StringList tagList;
    [StringPopup(new string[] { "tagList", "tags" })]
    public int customTag;

    public CustomTagItem(StringList tagList, int customTag)
    {
        this.tagList = tagList;
        this.customTag = customTag;
    }

    public override bool Equals(object other)
    {
        if (!(other is CustomTagItem)) return false;
        return Equals((CustomTagItem)other);
    }

    public bool Equals(CustomTagItem other)
    {
        return (tagList == other.tagList)
            && (customTag == other.customTag);
    }

    public override int GetHashCode()
    {
        return tagList.GetHashCode() * 31 + customTag.GetHashCode();
    }

    public static bool operator ==(CustomTagItem o1, CustomTagItem o2)
    {
        return o1.Equals(o2);
    }

    public static bool operator !=(CustomTagItem o1, CustomTagItem o2)
    {
        return !o1.Equals(o2);
    }

    public bool Check(GameObject other)
    {
        if (tagList == null) return true;

        if (CustomTag.IsItemActiveTagged(this) &&
            CustomTag.GetActiveTagged(this).Contains(other))
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
            if (Equals(tagComponent.item))
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

    public CustomTagItem GetCustomTag(int id)
    {
        return new CustomTagItem(tagList, id);
    }

    public bool IsInList()
    {
        bool hasTag = false;
        foreach (int tag in customTags)
        {
            CustomTagItem item = GetCustomTag(tag);
            if (CustomTag.IsItemActiveTagged(item))
            {
                hasTag = true;
                break;
            }
        }
        return hasTag;
    }

    public bool Check(GameObject other)
    {
        if (tagList == null) return true;

        bool hasTag = false;
        foreach (int tag in customTags)
        {
            CustomTagItem item = GetCustomTag(tag);
            if (CustomTag.IsItemActiveTagged(item) &&
                CustomTag.GetActiveTagged(item).Contains(other))
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
            if (tagComponent.enabled && (tagComponent.item.tagList == tagList) &&
                customTags.Contains(tagComponent.item.customTag))
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
