using UnityEngine;

public class GamepadSwitch_Events : GamepadSwitch
{
    [SerializeField]
    int defaultGamepad = -1;
    [SerializeField]
    DXEvent switchedToKeyboard = null;
    [SerializeField]
    DXIntEvent switchedToGamepad = null;

    protected override bool CanSwitchValue()
    {
        return (switchedToKeyboard != null) || (switchedToGamepad != null);
    }

    protected override void SwitchToDefault()
    {
        if (defaultGamepad < 0) SwitchToKeyboard();
        else SwitchToGamepad(defaultGamepad);
    }

    protected override void SwitchToKeyboard()
    {
        switchedToKeyboard?.Invoke();
    }

    protected override void SwitchToGamepad(int gamepadId)
    {
        switchedToGamepad?.Invoke(gamepadId);
    }
}
