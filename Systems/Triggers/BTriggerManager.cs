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

    int count;
    List<NDCollider> colliders;
    protected NDCollider[] selfColliders;
    protected NDCollider selfCollider { get { return selfColliders[0]; } }

    protected virtual void Awake()
    {
        colliders = new List<NDCollider>();
        selfColliders = NDCollider.GetNDCollidersFrom(gameObject);
    }

    void FixedUpdate()
    {
        if (count > 0)
        {
            if (!HasEnabledCollider())
                Disable();

            for (int i = count - 1; i > -1; i--)
            {
                if ((colliders[i] == null) ||
                    (!colliders[i].enabled) ||
                    (!colliders[i].gameObject.activeInHierarchy))
                {
                    colliders.RemoveAt(i);
                    count--;
                    if (count == 0) OnTrigExit();
                    OnTrigExit(null);
                }
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
            NDCollider enterCol = other.ND();
            int prevCount = count;
            colliders.Add(enterCol);
            count++;
            if (prevCount == 0) OnTrigEnter();
            OnTrigEnter(enterCol);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsThisEnabled() && CheckCollision(other.gameObject))
        {
            NDCollider enterCol = other.ND();
            int prevCount = count;
            colliders.Add(enterCol);
            count++;
            if (prevCount == 0) OnTrigEnter();
            OnTrigEnter(enterCol);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsThisEnabled() && CheckCollision(other.gameObject))
        {
            NDCollider exitCol = other.ND();
            for (int i = count - 1; i >= 0; i--)
                if (colliders[i] == exitCol)
                {
                    colliders.RemoveAt(i);
                    count--;
                    break;
                }
            if (count == 0) OnTrigExit();
            OnTrigExit(exitCol);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (IsThisEnabled() && CheckCollision(other.gameObject))
        {
            NDCollider exitCol = other.ND();
            for (int i = count - 1; i >= 0; i--)
                if (colliders[i] == exitCol)
                {
                    colliders.RemoveAt(i);
                    count--;
                    break;
                }
            if (count == 0) OnTrigExit();
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
        if (count > 0)
            Disable();
    }

    void Disable()
    {
        foreach (NDCollider col in colliders)
            OnTrigExit(col);
        colliders.Clear();
        count = 0;
        OnTrigExit();
    }

    bool HasEnabledCollider()
    {
        if ((selfColliders == null) || (selfColliders.Length <= 0))
            return false;
        for (int i = 0; i < selfColliders.Length; i++)
            if ((selfColliders[i] != null) && selfColliders[i].enabled)
                return true;
        return false;
    }
}
