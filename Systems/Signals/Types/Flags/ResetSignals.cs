using UnityEngine;

[DefaultExecutionOrder(-10000)]
public class ResetSignals : MonoBehaviour
{
    [SerializeField]
    BaseSignal[] signals = null;
    [SerializeField]
    bool onEnable = false;

    void OnEnable()
    {
        if (onEnable)
            DoReset();
    }

    public void DoReset()
    {
        for (int i = 0; i < signals.Length; i++)
            signals[i].Reset();
    }
}
