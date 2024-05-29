using UnityEngine;

[DefaultExecutionOrder(1000)]
public class RendererDuplicator_AfterLate : MonoBehaviour
{
    public BRendererDuplicator original;

    void LateUpdate()
    {
        original.UpdateEvent();
    }
}
