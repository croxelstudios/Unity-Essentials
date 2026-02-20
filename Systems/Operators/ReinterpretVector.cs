using UnityEngine;
using Sirenix.OdinInspector;

public class ReinterpretVector : MonoBehaviour
{
    [SerializeField]
    Transform referenceTransform = null;
    [SerializeField]
    bool normalize = false;
    [SerializeField]
    float multiplier = 1f;
    [SerializeField]
    Vector3 vectorMultiplier = Vector3.one;
    [SerializeField]
    bool projectOnPlane = true;
    [SerializeField]
    [ShowIf("projectOnPlane")]
    Vector3 plane2DNormal = Vector3.up;
    [SerializeField]
    [ShowIf("projectOnPlane")]
    Vector3 plane2DUp = Vector3.forward;
    [SerializeField]
    [ShowIf("projectOnPlane")]
    bool local = false;
    [SerializeField]
    DXVectorEvent vectorEvent = null;

    public void Reinterpret(Vector2 input)
    {
        Reinterpret((Vector3)input);
    }

    public void Reinterpret(Vector3 input)
    {
        if (projectOnPlane)
            input = PlaneProjection(input.InterpretVector2(plane2DNormal, PlaneUp()));
        if (referenceTransform != null) input = referenceTransform.rotation * input;
        if (normalize) input = input.normalized;
        input.Scale(vectorMultiplier);
        input *= multiplier;
        vectorEvent?.Invoke(input);
    }

    Vector2 PlaneProjection(Vector3 input)
    {
        Vector3 localPlaneNormal = local ? transform.rotation * plane2DNormal : plane2DNormal;
        return input.InterpretVector3Back(localPlaneNormal, PlaneUp());
    }

    Vector3 PlaneUp()
    {
        if (local) return transform.rotation * plane2DUp;
        else return plane2DUp;
    }
}
