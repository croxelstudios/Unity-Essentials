using UnityEngine;

public class ColorSignalCaller : BBaseSignalCaller
{
    [SerializeField]
    Color color = Color.white; //Change type here

    public void ChangeColor(Color newColor)
    {
        color = newColor;
    }

    public void CallSignal(ColorSignal signal) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            signal.CallSignal(color, _tag); //Change type here
    }
}
