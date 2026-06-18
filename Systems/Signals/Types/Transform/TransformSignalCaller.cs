using UnityEngine;

public class TransformSignalCaller : BBaseSignalCaller
{
    [SerializeField]
    new Transform transform = null; //Change type here

    public void ChangeGameObject(Transform newObject)
    {
        transform = newObject;
    }

    public void CallSignal(TransformSignal signal) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signal.CallSignal(transform, _tag); //Change type here
    }
}
