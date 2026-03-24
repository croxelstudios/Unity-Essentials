using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BTriggerManager : MonoBehaviour
{
    //[SerializeField]
    //[Tooltip("Determines if it should also check the tag of the attached rigidbody")]
    bool checkRigidbodyTag = false;
    [SerializeField]
    [TagSelector]
    [Tooltip("Will fire on any collision if this array is empty")]
    string[] detectionTags = null;
    [SerializeField]
    CustomTagItems[] customTags = null;

    int count;
    List<NDCollider> colliders;
    protected NDCollider[] selfColliders;
    protected NDCollider selfCollider { get { return selfColliders[0]; } }
    protected CustomTag senderCustomTag;

    protected virtual void Awake()
    {
        colliders = new List<NDCollider>();
        selfColliders = NDCollider.GetNDCollidersFrom(gameObject);
        CustomTag[] selfCustomTags = GetComponents<CustomTag>();
        foreach (CustomTag ct in selfCustomTags)
            if (ct.item.tagList == null)
            {
                senderCustomTag = ct;
                break;
            }
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
        if (CheckCollisionBase(other) && CheckCollisionCustom(other))
            return true;
        return false;
    }

    bool CheckCollisionBase(GameObject other)
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

    bool CheckCollisionCustom(GameObject other)
    {
        //TO DO: This should work with this thing,
        //but unfortunately it causes issues when the object is deactivated in the same physics step
        //return customTag.Check(other);
        if (customTags.IsNullOrEmpty())
            return true;
        else foreach (CustomTagItems customTag in customTags)
                if (customTag.DirtyCheck(other))
                    return true;
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

    public void SetFirstCustomTag(int id)
    {
        customTags.SetFirstCustomTag(id);
#if UNITY_EDITOR
        if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
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
