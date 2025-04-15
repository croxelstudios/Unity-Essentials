using UnityEngine;
using System;
using Sirenix.OdinInspector;

[Serializable]
public struct OriginTarget
{
    //TO DO: Include local bool here to generate the paths here?
    [SerializeField]
    [TagSelector]
    [Tooltip("Tag used to find the transform in case it is not specified")]
    string targetTag;
    [SerializeField]
    [Tooltip("Target transform")]
    Transform _target;
    public Transform target { get { UpdateTarget(); return _target; } }
    [SerializeField]
    [Tooltip("False: The target is found through the tag. True: The 'origin' is found through the tag")]
    bool useTagForOrigin;
    [SerializeField]
    [HideIf("useTagForOrigin")]
    [Tooltip("The transform that will move or the transform that the movement is calculated from. By default, this object's transform.")]
    Transform _origin;
    public Transform origin { get { UpdateOrigin(); return _origin; } }
    bool originWaslookedFor;

    public OriginTarget(string targetTag, Transform origin = null, bool useTagForOrigin = false)
    {
        this.targetTag = targetTag;
        this.useTagForOrigin = useTagForOrigin;
        _target = useTagForOrigin ? origin : null;
        _origin = useTagForOrigin ? null : origin;
        originWaslookedFor = false;
    }

    public OriginTarget(bool useTagForOrigin)
    {
        this.targetTag = "Player";
        this.useTagForOrigin = useTagForOrigin;
        _target = null;
        _origin = null;
        originWaslookedFor = false;
    }

    void UpdateTarget()
    {
        if (_target == null) _target = FindWithTag.TrCheckEmpty(targetTag);
    }

    void UpdateOrigin()
    {
        if (_origin == null) originWaslookedFor = false;
        if (useTagForOrigin && !originWaslookedFor)
        {
            _origin = FindWithTag.TrCheckEmpty(targetTag);
            originWaslookedFor = true;
        }
    }

    public void SetDefaultOrigin(Transform origin)
    {
        if (_origin == null)
            _origin = origin;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public bool IsNotNull()
    {
        return (_target != null) && (_origin != null);
    }
}
