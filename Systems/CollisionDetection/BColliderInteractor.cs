using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BColliderInteractor : MonoBehaviour
{
    //[SerializeField]
    //[Tooltip("Determines if it should also check the tag of the attached rigidbody")]
    bool checkRigidbodyTag = false;
    [SerializeField]
    LayerMask layerMask = -1;
    [SerializeField]
    [TagSelector]
    [Tooltip("Will fire on any collision if this array is empty")]
    string[] detectionTags = null;
    [SerializeField]
    [PropertySpace(0f, 10f)]
    CustomTagItems[] customTags = null;

    protected NDCollider[] selfColliders;
    protected NDCollider selfCollider { get { return selfColliders[0]; } }
    protected CustomTag senderCustomTag;

    bool tagsNullOrEmpty;
    bool customTagsNullOrEmpty;

    protected virtual void Awake()
    {
        selfColliders = NDCollider.GetNDCollidersFrom(gameObject);
        CustomTag[] selfCustomTags = GetComponents<CustomTag>();
        foreach (CustomTag ct in selfCustomTags)
            if (ct.item.tagList == null)
            {
                senderCustomTag = ct;
                break;
            }
        tagsNullOrEmpty = detectionTags.IsNullOrEmpty();
        customTagsNullOrEmpty = customTags.IsNullOrEmpty();
    }

    protected bool CheckCollision(GameObject other)
    {
        return CheckCollision(other, out CustomTag otherTag);
    }

    protected virtual bool CheckCollision(GameObject other, out CustomTag otherTag)
    {
        if (CheckCollisionBase(other) && CheckCollisionCustom(other, out otherTag))
            return true;

        otherTag = null;
        return false;
    }

    bool CheckCollisionBase(GameObject other)
    {
        if ((tagsNullOrEmpty || detectionTags.Contains(other.tag))
            && layerMask.ContainsLayer(other.layer))
            return true;
        else if (checkRigidbodyTag)
        {
            NDRigidbody rigid = NDRigidbody.GetNDRigidbodyFrom(other, NDRigidbody.Scope.inParents);
            if ((rigid != null) && detectionTags.Contains(rigid.tag) &&
                layerMask.ContainsLayer(rigid.layer))
                return true;
        }
        return false;
    }

    bool CheckCollisionCustom(GameObject other, out CustomTag otherTag)
    {
        otherTag = null;
        if (customTagsNullOrEmpty)
            return true;
        else foreach (CustomTagItems customTag in customTags)
        //TO DO: It causes issues when the object is deactivated in the same physics step?
                if (customTag.Check(other, out CustomTag targetTag))
                {
                    otherTag = targetTag;
                    return true;
                }
        return false;
    }

    protected void LaunchCustomTag(CustomTag otherTag)
    {
        if ((senderCustomTag != null) && (otherTag != null))
            senderCustomTag.LaunchEvents(otherTag.item);
    }

    protected virtual bool HasEnabledCollider()
    {
        if ((selfColliders == null) || (selfColliders.Length <= 0))
            return false;

        for (int i = 0; i < selfColliders.Length; i++)
            if ((selfColliders[i] != null) && selfColliders[i].enabled)
                return true;

        return false;
    }

    protected bool IsThisEnabled()
    {
        return this.IsActiveAndEnabled() && HasEnabledCollider();
    }

    public void SetFirstCustomTag(int id)
    {
        if (!customTags.IsNullOrEmpty())
            customTags[0].SetFirstCustomTag(id);
#if UNITY_EDITOR
        if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
    }
}
