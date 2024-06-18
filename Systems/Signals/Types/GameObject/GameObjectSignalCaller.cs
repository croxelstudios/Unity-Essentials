using UnityEngine;

public class GameObjectSignalCaller : BBaseSignalCaller
{
    [SerializeField]
    new GameObject gameObject = null; //Change type here

    public void ChangeGameObject(GameObject newObject)
    {
        gameObject = newObject;
    }

    public void CallSignal(GameObjectSignal signal) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signal.CallSignal(gameObject, _tag); //Change type here
    }
}
