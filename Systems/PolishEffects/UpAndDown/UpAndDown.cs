using UnityEngine;
using System.Collections;

public class UpAndDown : BSinoidalTransform
{
    [SerializeField]
    Vector3 axis = Vector3.up;
    [SerializeField]
    bool worldSpace = false;
    [SerializeField]
    float amount = 1f;
    public float Amount { get { return amount; } set { amount = value; } }

    protected override void Transformation(float value)
    {
        transform.Translate(axis.normalized * value * amount,
            worldSpace ? Space.World : Space.Self);
    }

    public void ResetPosition()
    {
        ResetTransform();
    }
}
