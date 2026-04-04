using System;
using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TMP_DisplayFloat : TMP_BTextPreprocessor, ITextReplacer
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
    string format = "F0";
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
        if (format == "Time") valueText = TimeSpan.FromSeconds(finalValue).ToString("c");
        else if (format.Contains('D', StringComparison.CurrentCultureIgnoreCase) ||
            format.Contains('X', StringComparison.CurrentCultureIgnoreCase))
            valueText = ((int)finalValue).ToString(format);
        else valueText = finalValue.ToString(format);

        return valueText;
    }

    void OnValidate()
    {
        if (replaceText == "") Reset();
        SetValue(startValue);
    }

    public void Add(float amount)
    {
        SetValue(value + amount);
    }

    void SetValue(float value)
    {
        _value = value;
        UpdateText();
    }

    void Reset()
    {
        replaceText = ITextReplacer.DefaultReplaceText(gameObject);
    }
}
