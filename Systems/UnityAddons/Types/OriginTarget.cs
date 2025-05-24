using UnityEngine;
using System;
using Sirenix.OdinInspector;

[Serializable]
public struct OriginTarget
{
    //TO DO: Include local bool here to generate the paths here?
    public ObjectRef<Transform> target;
    public ObjectRef<Transform> origin;

    public OriginTarget(string targetTag, Transform origin = null)
    {
        target = new ObjectRef<Transform>("Target", targetTag);
        this.origin = new ObjectRef<Transform>("Origin", origin);
    }

    public void SetOrigin(Transform origin)
    {
        this.origin.Set(origin);
    }

    public void SetTarget(Transform target)
    {
        this.target.Set(target);
    }

    public bool IsNotNull()
    {
        return ((Transform)target != null) && ((Transform)origin != null);
    }
}
