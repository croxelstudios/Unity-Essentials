using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BTriggerManager : MonoBehaviour
{
    //[SerializeField]
    //[Tooltip("Determines if it should also check the tag of the attached rigidbody")]
    bool checkRigidbodyTag = false;
    [SerializeField]
    [TagSelector]
    [Tooltip("Will fire on any collision if this array is empty")]
    string[] detectionTags = null;

    List<NDCollider> colliders;
    NDCollider selfCollider;

    protected virtual void Awake()
    {
        colliders = new List<NDCollider>();
        selfCollider = NDCollider.GetNDColliderFrom(gameObject);
    }

    void FixedUpdate()
    {
        if (selfCollider.IsNull() || (!selfCollider.enabled))
            OnDisable();

        for (int i = colliders.Count - 1; i > -1; i--)
        {
            if (colliders[i].IsNull() || (!colliders[i].enabled) || (!colliders[i].gameObject.activeInHierarchy))
            {
                colliders.RemoveAt(i);
                if (colliders.Count == 0) OnTrigExit();
                OnTrigExit(new NDCollider());
            }
        }
    }

    bool IsThisEnabled()
    {
        return this.IsActiveAndEnabled() &&
            (!selfCollider.IsNull()) && selfCollider.enabled;
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsThisEnabled() && CheckCollision(other.gameObject))
        {
            NDCollider enterCol = new NDCollider(other);
            int prevCount = colliders.Count;
            colliders.Add(new NDCollider(other));
            if (prevCount == 0) OnTrigEnter();
            OnTrigEnter(enterCol);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsThisEnabled() && CheckCollision(other.gameObject))
        {
            NDCollider enterCol = new NDCollider(other);
            int prevCount = colliders.Count;
            colliders.Add(enterCol);
            if (prevCount == 0) OnTrigEnter();
            OnTrigEnter(enterCol);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsThisEnabled() && CheckCollision(other.gameObject))
        {
            NDCollider exitCol = new NDCollider(other);
            for (int i = colliders.Count - 1; i >= 0; i--)
                if (colliders[i].IsEqual(exitCol))
                {
                    colliders.RemoveAt(i);
                    break;
                }
            if (colliders.Count == 0) OnTrigExit();
            OnTrigExit(exitCol);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (IsThisEnabled() && CheckCollision(other.gameObject))
        {
            NDCollider exitCol = new NDCollider(other);
            for (int i = colliders.Count - 1; i >= 0; i--)
                if (colliders[i].IsEqual(exitCol))
                {
                    colliders.RemoveAt(i);
                    break;
                }
            if (colliders.Count == 0) OnTrigExit();
            OnTrigExit(exitCol);
        }
    }

    protected virtual bool CheckCollision(GameObject other)
    {
        if (detectionTags.Contains(other.tag) || detectionTags == null || detectionTags.Length == 0)
            return true;
        else if (checkRigidbodyTag)
        {
            NDRigidbody rigid = NDRigidbody.GetNDRigidbodyFrom(other, NDRigidbody.Scope.inParents);
            if ((rigid != null) && detectionTags.Contains(rigid.tag))
                return true;
        }
        return false;
    }

    public virtual void OnTrigEnter()
    {

    }

    public virtual void OnTrigExit()
    {

    }

    public virtual void OnTrigEnter(NDCollider other)
    {

    }

    public virtual void OnTrigExit(NDCollider other)
    {

    }

    void OnDisable()
    {
        if (colliders.Count > 0)
        {
            foreach (NDCollider col in colliders)
                OnTrigExit(col);
            colliders.Clear();
            OnTrigExit();
        }
    }
}
