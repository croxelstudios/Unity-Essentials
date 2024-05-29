using System;
using UnityEngine;

[Serializable]
public struct TransformData
{
    public Vector3 position;
    public Vector3 eulerAngles;
    public Vector3 localScale;
    public Quaternion rotation
    {
        get { return Quaternion.Euler(eulerAngles); }
        set { eulerAngles = value.eulerAngles; }
    }
    public Vector3 lossyScale
    {
        get
        {
            if (parent == null) return localScale;
            else return Vector3.Scale(localScale, parent.lossyScale);
        }
    }
    public Transform parent;

    public TransformData(Transform source, bool locally = false)
    {
        if (locally)
        {
            position = source.localPosition;
            eulerAngles = source.localEulerAngles;
            localScale = source.localScale;
        }
        else
        {
            position = source.position;
            eulerAngles = source.eulerAngles;
            localScale = source.lossyScale;
        }
        parent = source.parent;
    }

    public TransformData(Vector3 position, Quaternion rotation, Vector3 localScale)
    {
        this.position = position;
        eulerAngles = rotation.eulerAngles;
        this.localScale = localScale;
        parent = null;
    }

    public TransformData(Vector3 position, Vector3 eulerAngles, Vector3 localScale)
    {
        this.position = position;
        this.eulerAngles = eulerAngles;
        this.localScale = localScale;
        parent = null;
    }

    public TransformData(Vector3 position, Quaternion rotation, Vector3 localScale, Transform parent)
    {
        this.position = position;
        eulerAngles = rotation.eulerAngles;
        this.localScale = localScale;
        this.parent = parent;
    }

    public TransformData(Vector3 position, Vector3 eulerAngles, Vector3 localScale, Transform parent)
    {
        this.position = position;
        this.eulerAngles = eulerAngles;
        this.localScale = localScale;
        this.parent = parent;
    }

    public TransformData GetOffsetFrom(Transform origin, bool opposite = false)
    {
        return opposite ? new TransformData(origin.position - position,
            origin.rotation * Quaternion.Inverse(rotation),
            origin.lossyScale - localScale) : new TransformData(position - origin.position,
            rotation * Quaternion.Inverse(origin.rotation),
            localScale - origin.lossyScale);
    }

    public TransformData GetOffsetFrom(TransformData origin, bool opposite = false)
    {
        return opposite ? new TransformData(origin.position - position,
            origin.rotation * Quaternion.Inverse(rotation),
            origin.localScale - localScale) : new TransformData(position - origin.position,
            rotation * Quaternion.Inverse(origin.rotation),
            localScale - origin.localScale);
    }

    public void AddTo(Transform target, bool locally = false)
    {
        if (locally)
        {
            target.localPosition += position;
            target.localRotation = rotation * target.rotation;
            target.localScale += localScale;
        }
        else
        {
            target.position += position;
            target.rotation = rotation * target.rotation;
            target.localScale += localScale;
        }
    }

    public void AddTo(TransformData target)
    {
        target.position += position;
        target.rotation = rotation * target.rotation;
        target.localScale += localScale;
    }

    public void SetInTransform(Transform target, bool locally = false)
    {
        if (locally)
        {
            target.localPosition = position;
            target.localRotation = rotation;
            target.localScale = localScale;
        }
        else
        {
            target.position = position;
            target.rotation = rotation;
            //target.localScale = localScale; //TO DO: Calculate lossyScale to localScale relation
        }
    }
}
