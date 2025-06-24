using UnityEngine;

public class PositionFromMagnitude : MonoBehaviour
{
    [SerializeField]
    Vector3 direction = Vector3.forward;
    [SerializeField]
    float multiplier = 1f;

    public void Position(float magnitude)
    {
        transform.localPosition = magnitude * direction * multiplier;
    }
}
