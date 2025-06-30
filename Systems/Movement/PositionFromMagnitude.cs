using Mono.CSharp;
using UnityEngine;

public class PositionFromMagnitude : MonoBehaviour
{
    [SerializeField]
    Space space = Space.Self;
    [SerializeField]
    Vector3 direction = Vector3.forward;
    [SerializeField]
    float multiplier = 1f;

    public void Position(float magnitude)
    {
        Vector3 movement = magnitude * direction * multiplier;
        transform.localPosition = (space == Space.World) ?
            transform.InverseTransformVector(movement) : movement;
    }
}
