public class Animator_SetBoolParameter : BAnimator_SetParameter<bool>
{
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
            InternalSet(parameter, value);
        }
    }

    public void SetBool(bool value)
    {
        UpdateNullAnimator();
        if (AnimatorIsValid())
            InternalSet(parameter, value);
    }

    protected override void InternalSet(string parameter, bool value)
    {
        animator.SetBool(parameter, value);
    }

    protected override void Reset()
    {
        base.Reset();
        parameter = "Bool";
    }
}
