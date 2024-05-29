using UnityEngine;

public class FloatSignalCaller : BBaseSignalCaller
{
    [SerializeField]
    float value = 0f; //Change type here

    public void CallSignal(FloatSignal signal) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signal.CallSignal(value, _tag); //Change type here
    }
}
