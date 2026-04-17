using System.Collections.Generic;
using UnityEngine;

public class BTriggerManager : BColliderInteractor
{
    int count;
    protected List<NDCollider> colliders;

    protected override void Awake()
    {
        colliders = new List<NDCollider>();
        base.Awake();
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

    void OnTriggerEnter(Collider other)
    {
        if (IsThisEnabled() && CheckCollision(other.gameObject, out CustomTag otherTag))
        {
            NDCollider enterCol = other.ND();
            int prevCount = count;
            colliders.Add(enterCol);
            count++;
            if (prevCount == 0) OnTrigEnter();
            OnTrigEnter(enterCol);
            LaunchCustomTag(otherTag);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsThisEnabled() && CheckCollision(other.gameObject, out CustomTag otherTag))
        {
            NDCollider enterCol = other.ND();
            int prevCount = count;
            colliders.Add(enterCol);
            count++;
            if (prevCount == 0) OnTrigEnter();
            OnTrigEnter(enterCol);
            LaunchCustomTag(otherTag);
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
}
