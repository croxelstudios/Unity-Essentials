using UnityEngine;

public class ScaleUpAndDown : BSinoidalTransform
{
    [SerializeField]
    Vector3 amount = Vector3.one * 0.1f;

    protected override void Awake()
    {
        amount = Vector3.Scale(amount, transform.localScale);
        base.Awake();
    }

    protected override void Transformation(float value)
    {
        transform.localScale += value * amount;
    }

    public void ResetScale()
    {
        ResetTransform();
    }
}
