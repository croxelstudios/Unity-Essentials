using Sirenix.OdinInspector;
using UnityEngine;

public class TriggerEvents : BTriggerManager
{
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
                CheckCollision(toRecover.gameObject, out CustomTag otherTag);
                entered?.Invoke();
                LaunchCustomTag(otherTag);
            }
        }
        UpdateToTriggerObjects();
    }

    void UpdateToTriggerObjects()
    {
        if ((!toTrigger.IsNullOrEmpty()) && (!colliders.IsNullOrEmpty()))
        {
            Vector3 pos = Vector3.zero;
            Vector4 rot = Vector4.zero;
            for (int i = 0; i < colliders.Count; i++)
            {
                Transform tr = colliders[i].transform;
                pos += tr.position;
                rot += tr.rotation.ToVector();
            }
            pos /= colliders.Count;
            rot /= colliders.Count;
            Quaternion qRot = rot.ToQuaternion();
            qRot.Normalize();
            for (int i = 0; i < toTrigger.Length; i++)
            {
                toTrigger[i].position = pos;
                toTrigger[i].rotation = qRot;
            }
        }
    }
}
