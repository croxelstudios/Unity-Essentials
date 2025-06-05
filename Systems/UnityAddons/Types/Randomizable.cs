using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
[HideLabel]
[InlineProperty]
public struct Randomizable
{
    string name;
    float randomizedValue;
    bool wasRandomized;
    [HideInInspector]
    public float min;
    [SerializeField]
    [HorizontalGroup(GroupName = "high", LabelWidth = 100f)]
    [LabelText("@GetValueLabel()")]
    [OnValueChanged("ApplyMin")]
    float value;
    [SerializeField]
    [HorizontalGroup(GroupName = "high", LabelWidth = 30f)]
    [ShowIf("randomize")]
    [OnValueChanged("ApplyMin")]
    float max;
    [SerializeField]
    [ShowIf("randomize")]
    [HorizontalGroup(GroupName = "high", LabelWidth = 35f, Width = 50f)]
    bool once;
    [SerializeField]
    [HorizontalGroup(GroupName = "high", LabelWidth = 70f, Width = 90f)]
    bool randomize;

    public Randomizable(string name, float minAtr, float value)
    {
        this.name = name;
        randomizedValue = value;
        wasRandomized = false;
        min = minAtr;
        this.value = value;
        randomize = false;
        once = false;
        max = value;
    }

    public Randomizable(string name, float minAtr, float min, float max, bool randomizeOnce = false)
    {
        this.name = name;
        randomizedValue = min;
        wasRandomized = false;
        this.min = minAtr;
        value = min;
        randomize = true;
        this.once = randomizeOnce;
        this.max = max;
    }

    public string GetValueLabel()
    {
        return randomize ? name + " Min" : name + "    ";
    }

    public void ApplyMin()
    {
        value = Mathf.Max(min, value);
        max = Mathf.Max(value, max);
    }

    public float GetValue()
    {
        if (randomize)
        {
            if (!once || !wasRandomized)
            {
                randomizedValue = Random.Range(value, max);
                wasRandomized = true;
            }
            return randomizedValue;
        }
        else return value;
    }

    public void Reset()
    {
        if (once)
            wasRandomized = false;
    }

    public static implicit operator float(Randomizable obj) => obj.GetValue();
}

//public class MiClaseAttributeProcessor : OdinAttributeProcessor<Randomizable>
//{
//    public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
//    {
//        if (member.Name == "value")
//        {
//            Randomizable myProp = (parentProperty.ValueEntry as IPropertyValueEntry<Randomizable>).SmartValue;
//            string label = myProp.GetValueLabel();
//            float width = GetValueLabelSize(label);
//            attributes.Add(new LabelWidthAttribute(width));
//        }
//    }

//    public float GetValueLabelSize(string text)
//    {
//        GUIStyle labelStyle = GUI.skin.label;
//        return labelStyle.CalcSize(new GUIContent(text)).x;
//    }
//}
