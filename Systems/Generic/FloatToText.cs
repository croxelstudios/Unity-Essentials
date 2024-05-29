using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FloatToText : MonoBehaviour
{
    Text text;

    [SerializeField]
    float startValue = 0f;
    [SerializeField]
    float scale = 1f;
    [SerializeField]
    float offset = 0f;
    [SerializeField]
    string preText = "";
    [SerializeField]
    string postText = "%";

    float _value;
    public float value
    {
        get { return _value; }
        set { SetValue(value); }
    }
    public int ivalue
    {
        get { return (int)_value; }
        set { SetValue(value); }
    }

    void Awake()
    {
        text = GetComponent<Text>();
    }

    void OnEnable()
    {
        SetValue(startValue);
    }

    public void Add(float amount)
    {
        SetValue(value + amount);
    }

    void SetValue(float value)
    {
        _value = value;
        if (text != null)
            text.text = preText + ((value * scale) + offset).ToString("F0") + postText;
    }

    public float GetValue()
    {
        return value;
    }
}
