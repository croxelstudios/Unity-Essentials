using UnityEngine;

public class EventSignalCaller : BBaseSignalCaller
{
    public void CallEventSignal(EventSignal signal) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signal.CallSignal(_tag); //Change type here
    }
}
