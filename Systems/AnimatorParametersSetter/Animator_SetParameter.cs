using Sirenix.OdinInspector;
using UnityEngine;

public class Animator_SetParameter : BAnimator_SetParameter
{
    [SerializeField]
    bool clampAnimationTime = true;

    public void ResetTrigger()
    {
        ResetTrigger(parameter);
    }

    public void TrySetTrigger()
    {
        TrySetTrigger(parameter);
    }

    public void SetTrigger()
    {
        SetTrigger(parameter);
    }

    public void ResetTrigger(string parameter)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
            animator.ResetTrigger(parameter);
    }

    public void TrySetTrigger(string parameter)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
            animator.SetTrigger(parameter);
    }

    public void SetTrigger(string parameter)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
        {
            animator.ResetTrigger(parameter);
            animator.SetTrigger(parameter);
        }
    }

    public void SetFloat(int value)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
        {
            if (clampAnimationTime) ClampAnimationTime();
            animator.SetFloat(parameter, value);
        }
    }

    public void SetFloat(float value)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
        {
            if (clampAnimationTime) ClampAnimationTime();
            animator.SetFloat(parameter, value);
        }
    }

    void ClampAnimationTime()
    {
        for (int i = 0; i < animator.layerCount; i++)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(i);
            animator.Play(stateInfo.shortNameHash, i, Mathf.Clamp01(stateInfo.normalizedTime));
        }
    }

    public void SetInt(float value)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
            animator.SetInteger(parameter, Mathf.FloorToInt(value));
    }

    public void SetInt(int value)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
            animator.SetInteger(parameter, value);
    }

    public void SwitchBool()
    {
        SwitchBool(parameter);
    }

    public void SwitchBool(string parameter)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
        {
            bool value = !animator.GetBool(parameter);
            animator.SetBool(parameter, value);
        }
    }

    public void SetBool(bool value)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
            animator.SetBool(parameter, value);
    }
}

public class BAnimator_SetParameter : MonoBehaviour
{
    [SerializeField]
    bool useAvailableChildAnimator = false;
    [SerializeField]
    [EnableIf("@useAvailableChildAnimator == false")]
    protected Animator animator = null;
    [SerializeField]
    protected string parameter = "State";
    public string parameterName { set { parameter = value; } }

    protected virtual void Awake()
    {
        if (useAvailableChildAnimator)
            animator = null;
    }

    protected virtual void Reset()
    {
        animator = GetComponentInParent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    protected void UpdateNullAnimator()
    {
        if (useAvailableChildAnimator && !AnimatorIsValid())
            animator = GetComponentInChildren<Animator>();
    }

    protected bool AnimatorIsValid()
    {
        return (animator != null) && animator.isActiveAndEnabled;
    }
}

public class BAnimator_SetParameter<T> : BAnimator_SetParameter
{
    public void Set(T value)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
            InternalSet(parameter, value);
    }

    protected virtual void InternalSet(string parameter, T value)
    {

    }
}
