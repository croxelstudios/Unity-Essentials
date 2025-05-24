using UnityEngine;
using System;
using Sirenix.OdinInspector;

[Serializable]
public struct OriginTarget
{
    //TO DO: Include local bool here to generate the paths here?
    public ObjectRef<Transform> target;
    public ObjectRef<Transform> origin;

    //vvv Delete vvv
    [SerializeField]
    [TagSelector]
    [Tooltip("Tag used to find the transform in case it is not specified")]
    string targetTag;
    [SerializeField]
    [Tooltip("Target transform")]
    Transform _target;
    //public Transform target { get { UpdateTarget(); return _target; } }
    [SerializeField]
    [Tooltip("False: The target is found through the tag. True: The 'origin' is found through the tag")]
    bool useTagForOrigin;
    [SerializeField]
    [HideIf("useTagForOrigin")]
    [Tooltip("The transform that will move or the transform that the movement is calculated from. By default, this object's transform.")]
    Transform _origin;
    //public Transform origin { get { UpdateOrigin(); return _origin; } }

    public OriginTarget(string targetTag, Transform origin = null, bool useTagForOrigin = false)
    {
        target = new ObjectRef<Transform>("Target", targetTag);
        this.origin = new ObjectRef<Transform>("Origin", origin);
        this.targetTag = targetTag;
        this.useTagForOrigin = useTagForOrigin;
        _target = useTagForOrigin ? origin : null;
        _origin = useTagForOrigin ? null : origin;
    }

    public OriginTarget(bool useTagForOrigin)
    {
        target = new ObjectRef<Transform>("Target", "Player");
        origin = new ObjectRef<Transform>("Origin", "Player");
        targetTag = "Player";
        this.useTagForOrigin = useTagForOrigin;
        _target = null;
        _origin = null;
    }

    public void SetDefaultOrigin(Transform origin)
    {
        if (_origin == null)
            _origin = origin;
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
