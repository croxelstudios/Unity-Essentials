public class Animator_SetTriggerParameter : BAnimator_SetParameter
{
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
}
