using UnityEngine;
using UnityEngine.Events;

public class GamepadSwitch_Updater : MonoBehaviour
{
    public BControllersData controllersData = null;
    public UnityEvent gamepadSwitch = new UnityEvent();

    void Awake()
    {
        gamepadSwitch = new UnityEvent();
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (controllersData.UpdateData())
            gamepadSwitch?.Invoke();
    }
}
