using System;
using UnityEngine;
using UnityEngine.Events;

public class GetChildOfTransformBy_Reference : MonoBehaviour
{
    [SerializeField]
    DXTransformEvent sendChildTransform = null;

    public void GetReferencedChildOn(Transform target)
    {
        Transform referencedChild = target.GetComponentInChildren<Reference>()?.transform;
        if (referencedChild != null) sendChildTransform?.Invoke(referencedChild);
    }
}
