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
    NDCollider[] selfColliders;

    protected virtual void Awake()
    {
        colliders = new List<NDCollider>();
        selfColliders = NDCollider.GetNDCollidersFrom(gameObject);
    }

    void FixedUpdate()
    {
        if (!HasEnabledCollider())
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
        return this.IsActiveAndEnabled() && HasEnabledCollider();
    }

    void OnTriggerEnter(Collider other)
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
                if (colliders[i] == exitCol)
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
                if (colliders[i] == exitCol)
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
        if (detectionTags == null || detectionTags.Contains(other.tag) || detectionTags.Length == 0)
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

    bool HasEnabledCollider()
    {
        if ((selfColliders == null) || (selfColliders.Length <= 0))
            return false;
        for (int i = 0; i < selfColliders.Length; i++)
            if ((!selfColliders[i].IsNull()) && selfColliders[i].enabled)
                return true;
        return false;
    }
}
