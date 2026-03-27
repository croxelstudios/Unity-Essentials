using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TMP_DisplayFloat : TMP_BTextPreprocessor
{
    [SerializeField]
    float startValue = 0f;
    [SerializeField]
    float scale = 1f;
    [SerializeField]
    float offset = 0f;
    [SerializeField]
    string replaceText = "{0}";
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

    protected override string ProcessText(string text)
    {
        return text.Replace(replaceText,
            GetResultingText().Replace("\\n", "\n").Replace("\\t", "\t"));
    }

    string GetResultingText()
    {
        float finalValue = (value * scale) + offset;

        string valueText;
        if (mode == "Time") valueText = TimeSpan.FromSeconds(finalValue).ToString("c");
        else valueText = finalValue.ToString(mode);

        return valueText;
    }

    void OnValidate()
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
    }

    void Reset()
    {
        replaceText = "{" + Count() + "}";
    }
}
