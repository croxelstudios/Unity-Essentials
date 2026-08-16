using Sirenix.OdinInspector;
using UnityEngine;

public class TriggerEvents : BTriggerManager
{
    [SerializeField]
    [Tooltip("If enabled, this will ignore trigger-trigger interactions and only work on trigger-solid interactions")]
    bool onlySolids = false;
    [SerializeField]
    [Tooltip("Determines if it should launch events when more than one collision with these tags ocurr")]
    bool fuseColliders = true;
    [Indent]
    [HideIf("fuseColliders")]
    [SerializeField]
    int maxCollisions = 1;
    [SerializeField]
    Transform[] toTrigger = null;
    [SerializeField]
    protected DXEvent entered = null;
    [SerializeField]
    protected DXEvent exited = null;

    protected override bool CheckCollision(NDCollider other, out CustomTag otherTag)
    {
        return base.CheckCollision(other, out otherTag) && (!(onlySolids && other.isTrigger));
    }

    public override void OnTrigEnter()
    {
        if (fuseColliders)
            entered?.Invoke();
    }

    public override void OnTrigExit()
    {
        if (fuseColliders)
            exited?.Invoke();
    }

    public override void OnTrigEnter(NDCollider other)
    {
        if (!fuseColliders)
        {
            if (count > maxCollisions)
                exited?.Invoke();
            entered?.Invoke();
        }
        UpdateToTriggerObjects();
    }

    public override void OnTrigExit(NDCollider other)
    {
        UpdateToTriggerObjects();
        if (!fuseColliders)
        {
            exited?.Invoke();

            int i = count - maxCollisions;
            NDCollider toRecover = null;
            while (count >= maxCollisions)
            {
                toRecover = Get(i);
                if (toRecover == null)
                {
                    RemoveAt(i);
                    i--;
                }
                else break;
            }

            if (count >= maxCollisions)
            {
                CheckCollision(toRecover, out CustomTag otherTag);
                entered?.Invoke();
                LaunchCustomTag(otherTag);
            }
        }
    }

    void UpdateToTriggerObjects()
    {
        if ((!toTrigger.IsNullOrEmpty()) && (!colliders.IsNullOrEmpty()))
        {
            for (int i = 0; i < toTrigger.Length; i++)
            {
                Transform tr = toTrigger[i];
                NDCollider collider = colliders[i % colliders.Count];
                Transform colTr = collider.transform;
                tr.position = colTr.position;
                tr.rotation = colTr.rotation;
            }
            if (toTrigger.Length < colliders.Count)
            {
                int i = toTrigger.Length - 1;
                Transform tr = toTrigger[i];
                Vector3 pos = Vector3.zero;
                Vector4 rot = Vector4.zero;
                for (int j = i; j < colliders.Count; j++)
                {
                    Transform colTr = colliders[j].transform;
                    pos += colTr.position;
                    rot += colTr.rotation.ToVector();
                }
                pos /= colliders.Count;
                rot /= colliders.Count;
                Quaternion qRot = rot.ToQuaternion();
                qRot.Normalize();
                for (int j = 0; j < toTrigger.Length; j++)
                {
                    toTrigger[j].position = pos;
                    toTrigger[j].rotation = qRot;
                }
            }
        }
    }
}
