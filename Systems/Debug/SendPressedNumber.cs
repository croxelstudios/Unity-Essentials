using UnityEngine;

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
        if ((Input.inputString.Length == 1) &&
            int.TryParse(Input.inputString, out int input))
            NumberActions?.Invoke((input + add) * multiply);
    }
}
