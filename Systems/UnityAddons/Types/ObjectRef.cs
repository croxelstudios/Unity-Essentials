using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

[Serializable]
[HideLabel]
[InlineProperty]
public struct ObjectRef<T> where T : Object
{
    string name;
    [SerializeField]
    [TagSelector]
    [ShowIf("@obj == null")]
    string tag;
    [SerializeField]
    [LabelText("@name")]
    [HorizontalGroup(DisableAutomaticLabelWidth = true)]
    T obj;

    public ObjectRef(string name, T obj)
    {
        this.name = name;
        tag = "";
        this.obj = obj;
    }

    public ObjectRef(string name, string tag)
    {
        this.name = name;
        this.tag = tag;
        obj = null;
    }

    [Button]
    [ShowIf("@obj != null")]
    [HorizontalGroup(DisableAutomaticLabelWidth = true, Width = 60f)]
    public void UseTag()
    {
        obj = null;
    }

    public static implicit operator T(ObjectRef<T> obj) => obj.GetObj();

    public string TagLabel()
    {
        return name + " Tag";
    }

    public void Set(T newObj)
    {
        obj = newObj;
    }

    void UpdateObj()
    {
        if (obj == null)
            obj = FindWithTag.Any<T>(tag);
    }

    T GetObj()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            UpdateObj();
        return obj;
    }
}
