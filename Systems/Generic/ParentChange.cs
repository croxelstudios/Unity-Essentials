using Sirenix.OdinInspector;
using UnityEngine;

public class ParentChange : MonoBehaviour
{
    [SerializeField]
    ObjectRef<Transform> targetParent = new ObjectRef<Transform>("Target Parent", "");
    [SerializeField]
    OnEnableBehaviour onEnableBehaviour = OnEnableBehaviour.SetNullParent;
    [SerializeField]
    DestroyBehaviour destroyBehaviour = DestroyBehaviour.None;
    [SerializeField]
    bool resetPosition = false;
    [SerializeField]
    [ShowIf("resetPosition")]
    Vector3 newLocalPosition = Vector3.zero;
    [SerializeField]
    bool resetRotation = false;
    [SerializeField]
    [ShowIf("resetRotation")]
    Vector3 newLocalEulerAngles = Vector3.zero;
    [SerializeField]
    bool resetScale = false;
    [SerializeField]
    [ShowIf("resetScale")]
    Vector3 newLocalScale = Vector3.one;
    [SerializeField]
    bool checkActiveState = true;

    bool hadParent = false;
    GameObject oldParent;

    enum OnEnableBehaviour { None, SetTargetParent, SetNullParent }
    enum DestroyBehaviour { None, DestroyWithOldParent, DestroyWithOldParentWhenParentless }

    void OnEnable()
    {
        switch (onEnableBehaviour)
        {
            case OnEnableBehaviour.SetTargetParent:
                SetParent();
                break;
            case OnEnableBehaviour.SetNullParent:
                SetParentToNull();
                break;
            default:
                break;
        }
    }

    void OnDisable()
    {
        switch (destroyBehaviour)
        {
            case DestroyBehaviour.DestroyWithOldParent:
            case DestroyBehaviour.DestroyWithOldParentWhenParentless:
                if (oldParent != null)
                    GenericCallbacks.Get(oldParent).onDestroy -= OldParentDestroyed;
                break;
            default:
                break;
        }
    }

    public void SetParentToNull()
    {
        SetOldParentToTrackDestruction();
        if (this.IsActiveAndEnabled() || !checkActiveState) transform.parent = null;
    }

    void OldParentDestroyed()
    {
        switch (destroyBehaviour)
        {
            case DestroyBehaviour.DestroyWithOldParent:
                Destroy(gameObject);
                break;
            case DestroyBehaviour.DestroyWithOldParentWhenParentless:
                if ((hadParent) && (transform.parent == null))
                    Destroy(gameObject);
                break;
            default:
                break;
        }
    }

    public void SetNewParent(Transform parent)
    {
        SetOldParentToTrackDestruction();
        if (this.IsActiveAndEnabled() || !checkActiveState) transform.SetParent(parent, true);
        ApplyLocalTransformChanges();
    }

    [TagSelector]
    public void SetParentByTag(string tag)
    {
        SetOldParentToTrackDestruction();
        if (this.IsActiveAndEnabled() || !checkActiveState)
            transform.parent = FindWithTag.GameObject(tag).transform;
        ApplyLocalTransformChanges();
    }

    public void SetParent()
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
        {
            SetOldParentToTrackDestruction();
            transform.SetParent(targetParent);
            ApplyLocalTransformChanges();
        }
    }

    void ApplyLocalTransformChanges()
    {
        if (resetPosition)
            transform.localPosition = newLocalPosition;
        if (resetRotation)
            transform.localEulerAngles = newLocalEulerAngles;
        if (resetScale)
            transform.localScale = newLocalScale;
    }

    void SetOldParentToTrackDestruction()
    {
        if (transform.parent != null)
        {
            switch (destroyBehaviour)
            {
                case DestroyBehaviour.DestroyWithOldParent:
                case DestroyBehaviour.DestroyWithOldParentWhenParentless:
                    oldParent = transform.parent.gameObject;
                    GenericCallbacks.Get(oldParent).onDestroy += OldParentDestroyed;
                    break;
                default:
                    break;
            }
            hadParent = true;
        }
    }

    public void SetParentUp(int iterations)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
        {
            SetOldParentToTrackDestruction();
            for (int i = 0; i < iterations; i++)
            {
                Transform parent = transform.parent;
                if (parent && parent.parent)
                    transform.SetParent(parent.parent);
            }

            ApplyLocalTransformChanges();
        }
    }
}
