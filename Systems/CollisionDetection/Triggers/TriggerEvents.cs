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
            if (colliders.Count > maxCollisions)
                exited?.Invoke();
            entered?.Invoke();
        }
    }

    public override void OnTrigExit(NDCollider other)
    {
        if (!fuseColliders)
        {
            exited?.Invoke();
            if (colliders.Count >= maxCollisions)
            {
                NDCollider toRecover = colliders[colliders.Count - maxCollisions];
                while ((toRecover == null) && (colliders.Count >= maxCollisions))
                {
                    colliders.RemoveAt(colliders.Count - maxCollisions);
                    toRecover = colliders[colliders.Count - maxCollisions];
                }

                if (colliders.Count >= maxCollisions)
                {
                    CheckCollision(toRecover.gameObject, out CustomTag otherTag);
                    entered?.Invoke();
                    LaunchCustomTag(otherTag);
                }
            }
        }
    }
}
