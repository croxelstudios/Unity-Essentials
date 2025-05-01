using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[ExecuteAlways]
public class GamepadSwitch : MonoBehaviour
{
    [SerializeField]
    [OnValueChanged("ChangeCheck")]
    BControllersData controllersData = null;
    [SerializeField]
    [OnValueChanged("ChangeCheck")]
    protected int current = 0;
    [SerializeField]
    [OnValueChanged("ChangeCheck")]
    protected InputPrompt inputData = null;
    //TO DO: Connect and disconnect events

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
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
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
    }

    protected virtual void OnEnable()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            if ((slotData != null) && (inputData != null))
            {
                UpdateController();
                slotData.gamepadSwitch.AddListener(UpdateController);
            }
        }
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            if (slotData != null)
                slotData.gamepadSwitch.RemoveListener(UpdateController);
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) ChangeCheck();
        //else
#endif
        //UpdateController();
    }

    void UpdateController()
    {
        if (controllersData != null)
            SwitchValue(controllersData.IdentifyController());
    }

    void ChangeCheck()
    {
        if (inputData != null) SwitchValue(current);
    }

    public void SwitchValue(int newValue)
    {
        if (newValue < -1)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) current = -1;
            else
#endif
                current = (controllersData.IsGamepadConnected()) ? inputData.gamepadDefault : -1;
        }
        else current = newValue;

        if (current < 0) SwitchValue(inputData.keyboardTexture);
        else if (current >= inputData.buttonTextures.Length)
            SwitchValue(inputData.buttonTextures[inputData.gamepadDefault]);
        else SwitchValue(inputData.buttonTextures[current]);
    }

    public virtual void SwitchValue(InputPrompt.InputPromptTexture texture)
    {

    }
}
