using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public struct TransformData : IEquatable<TransformData>
{
    public Vector3 position;
    public Vector3 eulerAngles;
    public Vector3 localScale;
    public Quaternion rotation
    {
        get { return Quaternion.Euler(eulerAngles); }
        set { eulerAngles = value.eulerAngles; }
    }
    [HideIf("@true")]
    public Vector3 lossyScale;
    //TO DO: Doesn't work when rotated
    /*
    {
        get
        {
            if (parent == null) return localScale;
            else return Vector3.Scale(localScale, parent.lossyScale);
        }
    }
    */
    [HideIf("_hideParent")]
    public Transform parent;
    bool _hideParent;
    public bool hideParent { set { _hideParent = value; } }

    public TransformData(Transform source, bool locally = false)
    {
        if (source == null)
        {
            position = Vector3.zero;
            eulerAngles = Vector3.zero;
            lossyScale = localScale = Vector3.one;
            parent = null;
            _hideParent = false;
            return;
        }

        if (locally)
        {
            position = source.localPosition;
            eulerAngles = source.localEulerAngles;
        }
        else
        {
            position = source.position;
            eulerAngles = source.eulerAngles;
        }
        localScale = source.localScale;
        lossyScale = source.lossyScale;
        parent = source.parent;
        _hideParent = false;
    }

    public TransformData(Vector3 position, Quaternion rotation, Vector3 localScale)
    {
        this.position = position;
        eulerAngles = rotation.eulerAngles;
        lossyScale = this.localScale = localScale;
        parent = null;
        _hideParent = false;
    }

    public TransformData(Vector3 position, Vector3 eulerAngles, Vector3 localScale)
    {
        this.position = position;
        this.eulerAngles = eulerAngles;
        lossyScale = this.localScale = localScale;
        parent = null;
        _hideParent = false;
    }

    public TransformData(Vector3 position, Quaternion rotation, Vector3 localScale, Transform parent)
    {
        this.position = position;
        eulerAngles = rotation.eulerAngles;
        this.localScale = localScale;
        lossyScale = localScale;
        if (parent != null)
            lossyScale = Vector3.Scale(localScale, parent.lossyScale);
        this.parent = parent;
        _hideParent = false;
    }

    public TransformData(Vector3 position, Vector3 eulerAngles, Vector3 localScale, Transform parent)
    {
        this.position = position;
        this.eulerAngles = eulerAngles;
        this.localScale = localScale;
        lossyScale = localScale;
        if (parent != null)
            lossyScale = Vector3.Scale(localScale, parent.lossyScale);
        this.parent = parent;
        _hideParent = false;
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

    public TransformData LerpTo(Transform other, float t)
    {
        return new TransformData(
            Vector3.LerpUnclamped(position, other.position, t),
            Quaternion.SlerpUnclamped(rotation, other.rotation, t),
            Vector3.LerpUnclamped(localScale, other.localScale, t), parent);
    }

    public TransformData LerpTo(TransformData other, float t)
    {
        return new TransformData(
            Vector3.LerpUnclamped(position, other.position, t),
            Quaternion.SlerpUnclamped(rotation, other.rotation, t),
            Vector3.LerpUnclamped(localScale, other.localScale, t), parent);
    }

    public TransformData LerpFromZero(float t)
    {
        return new TransformData(
            Vector3.LerpUnclamped(Vector3.zero, position, t),
            Quaternion.SlerpUnclamped(Quaternion.identity, rotation, t),
            Vector3.LerpUnclamped(Vector3.zero, localScale, t), parent);
    }

    public TransformData LerpFromDefault(float t)
    {
        return new TransformData(
            Vector3.LerpUnclamped(Vector3.zero, position, t),
            Quaternion.SlerpUnclamped(Quaternion.identity, rotation, t),
            Vector3.LerpUnclamped(Vector3.one, localScale, t), parent);
    }

    public TransformData Add(Transform target, bool locally = false)
    {
        if (locally)
        {
            position += target.localPosition;
            rotation = rotation.Add(target.localRotation);
            localScale += target.localScale;
        }
        else
        {
            position += target.position;
            rotation = rotation.Add(target.rotation);
            localScale += target.localScale;
        }
        return this;
    }

    public TransformData Add(TransformData target)
    {
        position += target.position;
        rotation = rotation.Add(target.rotation);
        localScale += target.localScale;
        return this;
    }

    public TransformData Subtract(Transform target, bool locally = false)
    {
        if (locally)
        {
            position -= target.localPosition;
            rotation = rotation.Subtract(target.localRotation);
            localScale -= target.localScale;
        }
        else
        {
            position -= target.position;
            rotation = rotation.Subtract(target.rotation);
            localScale -= target.localScale;
        }
        return this;
    }

    public TransformData Subtract(TransformData target)
    {
        position -= target.position;
        rotation = rotation.Subtract(target.rotation);
        localScale -= target.localScale;
        return this;
    }

    public void AddTo(Transform target, bool locally = false)
    {
        if (locally)
        {
            target.localPosition += position;
            target.localRotation = target.rotation.Add(rotation);
            target.localScale += localScale;
        }
        else
        {
            target.position += position;
            target.rotation = target.rotation.Add(rotation);
            target.localScale += localScale;
        }
    }

    public void AddTo(ref TransformData target)
    {
        target.position += position;
        target.rotation = target.rotation.Add(rotation);
        target.localScale += localScale;
    }

    public void SubtractTo(Transform target, bool locally = false)
    {
        if (locally)
        {
            target.localPosition -= position;
            target.localRotation = Quaternion.Inverse(rotation) * target.rotation;
            target.localScale -= localScale;
        }
        else
        {
            target.position -= position;
            target.rotation = Quaternion.Inverse(rotation) * target.rotation;
            target.localScale -= localScale;
        }
    }

    public void SubtractTo(ref TransformData target)
    {
        target.position -= position;
        target.rotation = Quaternion.Inverse(rotation) * target.rotation;
        target.localScale -= localScale;
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

    public void Multiply(float factor)
    {
        position *= factor;
        localScale *= factor;
        lossyScale *= factor;

        rotation.ToAngleAxis(out float angle, out Vector3 axis);
        rotation = Quaternion.AngleAxis(angle * factor, axis);
    }

    public bool IsNotZero()
    {
        return (position != Vector3.zero)
            || (eulerAngles != Vector3.zero)
            || (localScale != Vector3.zero);
    }

    public bool IsNotDefault()
    {
        return (position != Vector3.zero)
            || (eulerAngles != Vector3.zero)
            || (localScale != Vector3.one);
    }

    public override bool Equals(object other)
    {
        if (!(other is TransformData)) return false;
        return Equals((TransformData)other);
    }

    public bool Equals(TransformData other)
    {
        return (position == other.position)
            && (eulerAngles == other.eulerAngles)
            && (localScale == other.localScale);
    }

    public override int GetHashCode()
    {
        return new Vector3Int(position.GetHashCode(), 
            eulerAngles.GetHashCode(), localScale.GetHashCode()).GetHashCode();
    }

    public static bool operator ==(TransformData o1, TransformData o2)
    {
        return o1.Equals(o2);
    }

    public static bool operator !=(TransformData o1, TransformData o2)
    {
        return !o1.Equals(o2);
    }
}
