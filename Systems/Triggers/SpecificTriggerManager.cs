using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpecificTriggerManager : MonoBehaviour
{
    [SerializeField]
    [TagSelector]
    protected string[] detectionTags = null;

    List<NDCollider> colliders;

    protected virtual void Awake()
    {
        colliders = new List<NDCollider>();
    }

    protected virtual void FixedUpdate()
    {
        for (int i = colliders.Count - 1; i > -1; i--)
        {
            if ((colliders[i].IsNull()) || (!colliders[i].enabled) || !colliders[i].gameObject.activeInHierarchy)
                OnExit(colliders[i]);
        }
    }

    public void OnTriggerEnter(Collider collision)
    {
        OnEnter(collision.ND());
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        OnEnter(collision.ND());
    }

    public void OnTriggerExit(Collider collision)
    {
        OnExit(collision.ND());
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        OnExit(collision.ND());
    }

    void OnEnter(NDCollider ndCol)
    {
        GameObject other;
        if (ndCol.attachedRigidbody != null) other = ndCol.attachedRigidbody.gameObject;
        else other = ndCol.gameObject;
        if (detectionTags.Contains(ndCol.tag))
            OnTrigEnter(other);

        colliders.Add(ndCol);
    }

    void OnExit(NDCollider ndCol)
    {
        if (!ndCol.IsNull())
        {
            GameObject other;
            if (ndCol.attachedRigidbody != null) other = ndCol.attachedRigidbody.gameObject;
            else other = ndCol.gameObject;
            if (detectionTags.Contains(ndCol.tag))
                OnTrigExit(other);
        }
        else OnTrigExit(null);
        colliders.Remove(ndCol);
    }

    public virtual void OnTrigEnter(GameObject other)
    {

    }

    public virtual void OnTrigExit(GameObject other)
    {

    }
}
