using UnityEngine;
using Sirenix.OdinInspector;

public class ReinterpretVector : MonoBehaviour
{
    [SerializeField]
    Transform referenceTransform = null;
    [SerializeField]
    float multiplier = 1f;
    [SerializeField]
    bool projectOnPlane = true;
    [SerializeField]
    [ShowIf("@projectOnPlane")]
    Vector3 plane2DNormal = Vector3.up;
    [SerializeField]
    [ShowIf("@projectOnPlane")]
    Vector3 plane2DUp = Vector3.forward;
    [SerializeField]
    [ShowIf("@projectOnPlane")]
    bool local = false;
    [SerializeField]
    DXVectorEvent vectorEvent = null;

    private void Awake()
    {
        if (local)
            plane2DUp = transform.rotation * plane2DUp;
    }

    public void Reinterpret(Vector2 input)
    {
        input *= multiplier;
        if (referenceTransform != null) input = referenceTransform.rotation * input;
        if (projectOnPlane)
            input = PlaneProjection(input.InterpretVector2(plane2DNormal, plane2DUp));
        vectorEvent?.Invoke(input);
    }

    public void Reinterpret(Vector3 input)
    {
        input *= multiplier;
        if (referenceTransform != null) input = referenceTransform.rotation * input;
        if (projectOnPlane)
            input = PlaneProjection(input.InterpretVector2(plane2DNormal, plane2DUp));
        vectorEvent?.Invoke(input);
    }

    Vector2 PlaneProjection(Vector3 input)
    {
        Vector3 localPlaneNormal = local ? transform.rotation * plane2DNormal : plane2DNormal;
        return input.InterpretVector3Back(localPlaneNormal, plane2DUp);
    }
}
