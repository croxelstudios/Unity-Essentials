using UnityEngine;

public class FloatSignalSum : MonoBehaviour
{
    [SerializeField]
    FloatSignal[] signals = null;
    [SerializeField]
    DXFloatEvent sumResult = null;

    [HideInInspector]
    public float totalValue;

    void OnEnable()
    {
        foreach (FloatSignal signal in signals)
            signal.called.AddListener(UpdateValue);
        UpdateValue();
    }

    void UpdateValue()
    {
        totalValue = 0f;
        foreach (FloatSignal signal in signals)
            totalValue += signal.currentValue;
        sumResult?.Invoke(totalValue);
    }
}
