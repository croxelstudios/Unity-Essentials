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
    float maxMagnitude = Mathf.Infinity;
    [SerializeField]
    Vector3 vectorMultiplier = Vector3.one;
    [SerializeField]
    bool projectOnPlane = true;
    [SerializeField]
    [ShowIf("projectOnPlane")]
    Vector3 planeNormal = Vector3.up;
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
            input = Vector3.ProjectOnPlane(input, PlaneNormal());
        if (referenceTransform != null) input = referenceTransform.rotation * input;
        if (normalize) input = input.normalized;
        if (input.sqrMagnitude > (maxMagnitude * maxMagnitude))
            input = input.normalized * maxMagnitude;
        input.Scale(vectorMultiplier);
        input *= multiplier;
        vectorEvent?.Invoke(input);
    }

    Vector3 PlaneNormal()
    {
        if (local) return transform.TransformDirection(planeNormal);
        else return planeNormal;
    }
}
