using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class BGamepadSwitch_Prompt : GamepadSwitch
{
    [SerializeField]
    [OnValueChanged("ChangeCheck")]
    protected InputPrompt inputData = null;
    //TO DO: Connect and disconnect events

    protected override int AvailableGamepads()
    {
        return inputData.buttonTextures.Length;
    }

    protected override bool CanSwitchValue()
    {
        return inputData != null;
    }

    protected override void SwitchToDefault()
    {
        SwitchToGamepad(inputData.gamepadDefault);
    }

    protected override void SwitchToKeyboard()
    {
        SwitchValue(inputData.keyboardTexture);
    }

    protected override void SwitchToGamepad(int gamepadId)
    {
        SwitchValue(inputData.buttonTextures[gamepadId]);
    }

    public virtual void SwitchValue(InputPrompt.InputPromptTexture texture)
    {

    }
}
