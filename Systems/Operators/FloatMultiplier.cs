using UnityEngine;

public class FloatMultiplier : MonoBehaviour
{
    [SerializeField]
    Component component = null;
    [SerializeField]
    string propertyName = "";
    [SerializeField]
    float _multiplier = 1f;
    public float multiplier
    {
        get { return _multiplier; }
        set
        {
            ResetValue();
            _multiplier = value;
            if (this.IsActiveAndEnabled()) Multiply();
        }
    }

    bool isMultiplied;

    void OnEnable()
    {
        Multiply();
    }

    void OnDisable()
    {
        ResetValue();
    }

    void Multiply()
    {
        if (!isMultiplied)
        {
            ReflectionTools.SetValue(component, propertyName,
            ReflectionTools.GetValue<float>(component, propertyName, false) * multiplier, false);
            isMultiplied = true;
        }
    }

    void ResetValue()
    {
        if (isMultiplied)
        {
            ReflectionTools.SetValue(component, propertyName,
            ReflectionTools.GetValue<float>(component, propertyName, false) / multiplier, false);
            isMultiplied = false;
        }
    }

    public void AddToMultiplier(float amount)
    {
        ResetValue();
        multiplier += amount;
        if (this.IsActiveAndEnabled()) Multiply();
    }
}
