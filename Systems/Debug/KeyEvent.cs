using UnityEngine.Events;
using UnityEngine;

public class KeyEvent : MonoBehaviour
{
    [SerializeField]
    KeyCode key = KeyCode.F;

    [SerializeField]
    DXEvent KeyPressed = null;
    [SerializeField]
    DXEvent KeyReleased = null;

    [SerializeField]
    bool toggle = false;

    bool isOn = false;

    void Update()
    {
        if (toggle)
        {
            if (Input.GetKeyDown(key))
            {
                if (isOn)
                {
                    KeyReleased?.Invoke();
                    isOn = false;
                }
                else
                {
                    KeyPressed?.Invoke();
                    isOn = true;
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(key)) KeyPressed?.Invoke();
            else if (Input.GetKeyUp(key)) KeyReleased?.Invoke();
        }
    }

    public void LaunchKeyPressedExternally()
    {
        KeyPressed?.Invoke();
    }

    public void LaunchKeyReleasedExternally()
    {
        KeyPressed?.Invoke();
    }
}
