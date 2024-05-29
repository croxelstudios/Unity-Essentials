using UnityEngine;

public class Vector3SignalCaller : BBaseSignalCaller
{
    [SerializeField]
    Vector3 value = Vector3.zero; //Change type here

    public void CallSignal(Vector3Signal signal) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signal.CallSignal(value, _tag); //Change type here
    }
}