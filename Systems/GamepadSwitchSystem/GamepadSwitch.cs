using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class GamepadSwitch : MonoBehaviour
{
    [SerializeField]
    [OnValueChanged("ChangeCheck")]
    BControllersData controllersData = null;
    [SerializeField]
    [OnValueChanged("ChangeCheck")]
    protected int current = 0;

    static Dictionary<BControllersData, GamepadSwitch_Updater> updaters;
    GamepadSwitch_Updater slotData
    {
        get
        {
            if ((updaters != null) && updaters.ContainsKey(controllersData))
                return updaters[controllersData];
            else return null;
        }
    }

    void Awake()
    {
        updaters = updaters.CreateIfNull();
        if (!updaters.ContainsKey(controllersData))
        {
            controllersData.AwakeData();
            GameObject go = new GameObject("GamepadSwitch_Updater");
            updaters.Add(controllersData, go.AddComponent<GamepadSwitch_Updater>());
            slotData.controllersData = controllersData;
        }
    }

    protected virtual void OnEnable()
    {
        if (slotData != null)
        {
            controllersData.UpdateData();
            UpdateController();
            slotData.gamepadSwitch.AddListener(UpdateController);
        }
    }

    private void OnDisable()
    {
        if (slotData != null)
            slotData.gamepadSwitch.RemoveListener(UpdateController);
    }

    void UpdateController()
    {
        if (controllersData != null)
            SwitchValue(controllersData.IdentifyController());
    }

    protected void ChangeCheck()
    {
        SwitchValue(current);
    }

    public void SwitchValue(int newValue)
    {
        if (CanSwitchValue())
        {
            if (newValue < -1)
            {
    #if UNITY_EDITOR
                if (!Application.isPlaying) current = -1;
                else
    #endif
                    current = controllersData.IsGamepadConnected() ? newValue : -1;
            }
            else current = newValue;

            if (current.IsBetween(-1, AvailableGamepads()))
            {
                if (current < 0) SwitchToKeyboard();
                else SwitchToGamepad(current);
            }
            else SwitchToDefault();
        }
    }

    protected virtual int AvailableGamepads()
    {
        return controllersData.AvailableGamepads();
    }

    protected virtual bool CanSwitchValue()
    {
        return true;
    }

    protected virtual void SwitchToDefault()
    {

    }

    protected virtual void SwitchToKeyboard()
    {

    }

    protected virtual void SwitchToGamepad(int gamepadId)
    {

    }
}
