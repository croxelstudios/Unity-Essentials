using UnityEngine;

public class Animator_SetFloatParameter : BAnimator_SetParameter<float>
{
    [SerializeField]
    bool clampAnimationTime = true;

    public void SetFloat(int value)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
            InternalSet(parameter, value);
    }

    public void SetFloat(float value)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
            InternalSet(parameter, value);
    }

    protected override void InternalSet(string parameter, float value)
    {
        if (clampAnimationTime) ClampAnimationTime();
        animator.SetFloat(parameter, value);
    }

    void ClampAnimationTime()
    {
        for (int i = 0; i < animator.layerCount; i++)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(i);
            animator.Play(stateInfo.shortNameHash, i, Mathf.Clamp01(stateInfo.normalizedTime));
        }
    }

    protected override void Reset()
    {
        base.Reset();
        parameter = "Speed";
    }
}
