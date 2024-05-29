using UnityEngine;
using Sirenix.OdinInspector;

public class ReinterpretVector : MonoBehaviour
{
    [SerializeField]
    Transform referenceTransform = null;
    //[SerializeField]
    //bool get2DDataVector = true; //TO DO: What was this for??
    [SerializeField]
    [ShowIf("@projectOn2D")]
    Vector3 plane2DNormal = Vector3.up;
    [SerializeField]
    [ShowIf("@projectOn2D")]
    Vector3 plane2DUp = Vector3.forward;
    [SerializeField]
    [ShowIf("@projectOn2D")]
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
        Reinterpret(input.InterpretVector2(plane2DNormal, plane2DUp));
    }

    public void Reinterpret(Vector3 input)
    {
        Vector3 localPlaneNormal = local ? transform.rotation * plane2DNormal : plane2DNormal;

        if (referenceTransform != null) input = referenceTransform.rotation * input;
        vectorEvent?.Invoke(input.InterpretVector3Back(localPlaneNormal, plane2DUp));
    }
}
