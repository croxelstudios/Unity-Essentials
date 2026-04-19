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
            if (count > maxCollisions)
                exited?.Invoke();
            entered?.Invoke();
        }
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
                if (toRecover.IsNull())
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
    }
}
