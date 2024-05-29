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
}
