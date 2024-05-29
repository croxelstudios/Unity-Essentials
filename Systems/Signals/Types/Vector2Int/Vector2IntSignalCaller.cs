using UnityEngine;

public class Vector2IntSignalCaller : BBaseSignalCaller
{
    [SerializeField]
    Vector2Int value = Vector2Int.zero; //Change type here

    public void CallSignal(Vector2IntSignal signal) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signal.CallSignal(value, _tag); //Change type here
    }
}