using UnityEngine;
using UnityEngine.Events;
using System;

public class SendPressedNumber : MonoBehaviour
{
    [SerializeField]
    int add = 0;
    [SerializeField]
    int multiply = 1;
    [SerializeField]
    DXIntEvent NumberActions = null;

    void Update()
    {
        int input;
        if ((Input.inputString.Length == 1) &&
            int.TryParse(Input.inputString, out input))
            NumberActions?.Invoke((input + add) * multiply);
    }
}
