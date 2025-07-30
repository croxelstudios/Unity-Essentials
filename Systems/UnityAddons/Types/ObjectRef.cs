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
    bool objWasNull;
    [SerializeField]
    [TagSelector]
    [ShowIf("@obj == null")]
    string tag;
    [SerializeField]
    [LabelText("@name")]
    [HorizontalGroup(DisableAutomaticLabelWidth = true)]
    T obj;

    T[] objs;

    public ObjectRef(string name, T obj)
    {
        this.name = name;
        tag = "";
        this.obj = obj;
        objWasNull = false;
        objs = null;
    }

    public ObjectRef(string name, string tag)
    {
        this.name = name;
        this.tag = tag;
        obj = null;
        objWasNull = false;
        objs = null;
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

    public void Reset()
    {
        obj = null;
        objs = null;
    }

    public void SetClosest(Transform target)
    {
        SetClosest(target.position);
    }

    public void SetClosest(Vector3 target)
    {
        if ((obj == null) || objWasNull)
        {
            if (objs == null)
                objs = FindWithTag.Anys<T>(tag);
            obj = objs.GetClosest(target);
        }
    }

    public Vector3 AveragePosition()
    {
        if ((obj == null) || objWasNull)
        {
            if (objs == null)
                objs = FindWithTag.Anys<T>(tag);
            return objs.AveragePosition();
        }
        else return obj.GetTransform().position;
    }

    void UpdateObj()
    {
        if (obj == null)
        {
            obj = FindWithTag.Any<T>(tag);
            objWasNull = true;
        }
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
