using UnityEngine;
using UnityEngine.Events;

public class TriggerEvents : BTriggerManager
{
    [SerializeField]
    [Tooltip("Determines if it should launch events when more than one collision with these tags ocurr")]
    bool fuseColliders = true;
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
            entered?.Invoke();
    }

    public override void OnTrigExit(NDCollider other)
    {
        if (!fuseColliders)
            exited?.Invoke();
    }
}
