using UnityEngine;

public class FlagSetter : BBaseSignalCaller
{
    [SerializeField]
    bool value = false; //Change type here

    public void SetFlag(Flag flag) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flag.SetFlag(value, _tag); //Change type here
    }

    public void SetToTrue(Flag flag)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flag.SetFlag(true, _tag); //Change type here
    }

    public void SetToFalse(Flag flag)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flag.SetFlag(false, _tag); //Change type here
    }

    public void SwitchValue(Flag flag)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flag.SwitchValue(); //Change type here
    }

    public void LaunchOnTrue(Flag flag)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flag.LaunchOnTrue(_tag); //Change type here
    }

    public void LaunchOnFalse(Flag flag)
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            flag.LaunchOnFalse(_tag); //Change type here
    }
}
