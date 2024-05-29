using UnityEngine;

public class IntSignalCaller : BBaseSignalCaller
{
    [SerializeField]
    int value = 0; //Change type here

    public void CallSignal(FloatSignal signal) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signal.CallSignal(value, _tag); //Change type here
    }
}
