using UnityEngine;

public class Animator_SetIntParameter : BAnimator_SetParameter<int>
{
    public void SetInt(float value)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
            InternalSet(parameter, Mathf.FloorToInt(value));
    }

    public void SetInt(int value)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
            InternalSet(parameter, value);
    }

    protected override void InternalSet(string parameter, int value)
    {
        animator.SetInteger(parameter, value);
    }

    protected override void Reset()
    {
        base.Reset();
        parameter = "State";
    }
}
