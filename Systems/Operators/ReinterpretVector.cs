using UnityEngine;
using Sirenix.OdinInspector;

public class ReinterpretVector : DXMonoBehaviour
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
    Vector3 vectorMax = Vector3.one * Mathf.Infinity;
    [SerializeField]
    bool projectOnPlane = true;
    [SerializeField]
    [Indent]
    [ShowIf("projectOnPlane")]
    Vector3 planeNormal = Vector3.up;
    [SerializeField]
    [Indent]
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
        if (this.IsActiveAndEnabled())
        {
            if (projectOnPlane)
                input = Vector3.ProjectOnPlane(input, PlaneNormal());
            if (referenceTransform != null) input = referenceTransform.rotation * input;
            if (normalize) input = input.normalized;
            input.Scale(vectorMultiplier);
            input *= multiplier;
            if (input.sqrMagnitude > (maxMagnitude * maxMagnitude))
                input = input.normalized * maxMagnitude;
            input = new Vector3(
                Mathf.Clamp(input.x, -vectorMax.x, vectorMax.x),
                Mathf.Clamp(input.y, -vectorMax.y, vectorMax.y),
                Mathf.Clamp(input.z, -vectorMax.z, vectorMax.z)
            );
            vectorEvent?.Invoke(input);
        }
    }

    Vector3 PlaneNormal()
    {
        if (local) return transform.TransformDirection(planeNormal);
        else return planeNormal;
    }

    public void SetMultiplier(float multiplier)
    {
        this.multiplier = multiplier;
    }

    public void SetMultiplierX(float multiplier)
    {
        vectorMultiplier.x = multiplier;
    }

    public void SetMultiplierY(float multiplier)
    {
        vectorMultiplier.y = multiplier;
    }

    public void SetMultiplierZ(float multiplier)
    {
        vectorMultiplier.z = multiplier;
    }

    public void SetMaxMagnitude(float maxMagnitude)
    {
        this.maxMagnitude = maxMagnitude;
    }

    public void SetMaxX(float max)
    {
        vectorMax.x = max;
    }

    public void SetMaxY(float max)
    {
        vectorMax.y = max;
    }

    public void SetMaxZ(float max)
    {
        vectorMax.z = max;
    }
}
