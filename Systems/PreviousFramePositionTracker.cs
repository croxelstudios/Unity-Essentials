using UnityEngine;

public class PreviousFramePositionTracker : MonoBehaviour
{
    Vector3 position;

    void LateUpdate()
    {
        position = transform.position;
    }

    public void MoveToPreviousPosition()
    {
        Vector3 displacement = transform.position - position;
        transform.Translate(-displacement, Space.World);
    }
}
