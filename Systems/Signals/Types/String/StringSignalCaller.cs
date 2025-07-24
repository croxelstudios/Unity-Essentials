using UnityEngine;

public class StringSignalCaller : BBaseSignalCaller
{
    [SerializeField]
    string value = ""; //Change type here

    public void CallSignal(StringSignal signal) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signal.CallSignal(value, _tag); //Change type here
    }

    public void SetCurrentScenePath(StringSignal signal) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signal.CallSignal(gameObject.scene.path, _tag); //Change type here
    }
}
