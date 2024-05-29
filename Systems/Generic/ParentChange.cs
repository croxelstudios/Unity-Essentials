using UnityEngine;

public class ParentChange : MonoBehaviour
{
    [SerializeField]
    Transform target;
    [SerializeField]
    [TagSelector]
    string targetTag = "";
    [SerializeField]
    bool searchEveryTime = true;
    [SerializeField]
    bool onEnable = false;
    [SerializeField]
    bool nullOnEnable = false;
    [SerializeField]
    bool changePositionAfterParentChange = false;
    [SerializeField]
    Vector3 positionAfterParentChange = Vector3.zero;
    [SerializeField]
    bool checkActiveState = true;
    [SerializeField]
    bool destroyWithOldParent = false;
    [SerializeField]
    bool destroyWithOldParentWhenIsNull = false;
    

    bool hadParent = false;
    GameObject oldParent;

    void OnEnable()
    {
        if ((target == null) && !searchEveryTime && !string.IsNullOrEmpty(targetTag))
            target = GameObject.FindGameObjectWithTag(targetTag)?.transform;
        if (nullOnEnable) SetParentToNull();
        else if (onEnable) SetParent();
    }

    public void SetParentToNull()
    {
        SetOldParentToBeDestroyed();
        if (this.IsActiveAndEnabled() || !checkActiveState) transform.parent = null;
    }

    void OldParentDestroyed()
    {
        if ((this && destroyWithOldParent) || (this && destroyWithOldParentWhenIsNull && hadParent && !transform.parent))
            Destroy(gameObject);
    }

    public void SetNewParent(Transform parent)
    {
        SetOldParentToBeDestroyed();
        if (this.IsActiveAndEnabled() || !checkActiveState) transform.parent = parent;
        if (changePositionAfterParentChange)
            transform.localPosition = positionAfterParentChange;
    }

    [TagSelector]
    public void SetParentByTag(string tag)
    {
        SetOldParentToBeDestroyed();
        if (this.IsActiveAndEnabled() || !checkActiveState) transform.parent = GameObject.FindGameObjectWithTag(tag).transform;
        if (changePositionAfterParentChange)
            transform.localPosition = positionAfterParentChange;
    }

    public void SetParent()
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
        {
            SetOldParentToBeDestroyed();
            if ((target == null) && searchEveryTime) target = GameObject.FindGameObjectWithTag(targetTag)?.transform;
            transform.SetParent(target);
            if (changePositionAfterParentChange)
                transform.localPosition = positionAfterParentChange;
        }
    }

    void SetOldParentToBeDestroyed()
    {
        if ((destroyWithOldParent || destroyWithOldParentWhenIsNull) && transform.parent)
        {
            oldParent = transform.parent.gameObject;
            oldParent.AddComponent<GenericCallbacks>();
            oldParent.GetComponent<GenericCallbacks>().onDestroy += OldParentDestroyed;
            hadParent = true;
        }
    }

    public void SetParentUp(int iterations)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
        {
            SetOldParentToBeDestroyed();
            for (int i = 0; i < iterations; i++)
            {
                Transform parent = transform.parent;
                if (parent && parent.parent)
                    transform.SetParent(parent.parent);
            }
            
            if (changePositionAfterParentChange)
                transform.localPosition = positionAfterParentChange;
        }
    }
}
