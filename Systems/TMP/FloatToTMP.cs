using UnityEngine;
using TMPro;
using System.Linq;
using System;

[RequireComponent(typeof(TMP_Text))]
public class FloatToTMP : MonoBehaviour
{
    TMP_Text text;

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
    [SerializeField]
    string mode = "F0";
    [SerializeField]
    int priority = 0;

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
        if (text == null)
        {
            text = GetComponent<TMP_Text>();
            SetValue(startValue);
        }
    }

    public void Add(float amount)
    {
        SetValue(value + amount);
    }

    void SetValue(float value)
    {
        _value = value;
        if (text == null) text = GetComponent<TMP_Text>();
        FloatToTMP[] currentFloatToTMPs = GetComponents<FloatToTMP>();
        currentFloatToTMPs = currentFloatToTMPs.OrderBy(x => x.priority).ToArray();
        string resultingText = "";
        for (int i = 0; i < currentFloatToTMPs.Length; i++)
            if (currentFloatToTMPs[i].IsActiveAndEnabled())
                resultingText += currentFloatToTMPs[i].
                    GetResultingText().Replace("\\n", "\n").Replace("\\t", "\t");
        text.SetText(resultingText);
    }

    public string GetResultingText()
    {
        float finalValue = (value * scale) + offset;

        string valueText;
        if (mode == "Time") valueText = TimeSpan.FromSeconds(finalValue).ToString("c");
        else valueText = finalValue.ToString(mode);

        return preText + valueText + postText;
    }
}
