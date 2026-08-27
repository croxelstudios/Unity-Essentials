using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using System.Reflection;
using System.Collections.Generic;
#endif

[Serializable]
[HideLabel]
[InlineProperty]
public struct Randomizable
{
    [HideInInspector]
    public string name;
    float randomizedValue;
    bool wasRandomized;
    [SerializeField]
    [HorizontalGroup]
    [LabelText("@GetValueLabel()")]
    [OnValueChanged("ApplyMin")]
    float value;
    [SerializeField]
    [HorizontalGroup(LabelWidth = 30f)]
    [ShowIf("randomize")]
    [OnValueChanged("ApplyMin")]
    float max;
    [HorizontalGroup(LabelWidth = 70f, Width = 90f)]
    public bool randomize;

    public float Max => GetMax();
    public float Min => value;

    public Randomizable(string name, float value)
    {
        this.name = name;
        randomizedValue = value;
        wasRandomized = false;
        this.value = value;
        randomize = false;
        max = value;
    }

    public Randomizable(string name, float min, float max)
    {
        this.name = name;
        randomizedValue = min;
        wasRandomized = false;
        value = min;
        randomize = true;
        this.max = max;
    }

    public Randomizable(float minAtr, string name, float min, float max)
    {
        this.name = name;
        randomizedValue = min;
        wasRandomized = false;
        value = min;
        randomize = true;
        this.max = max;
    }

    public string GetValueLabel()
    {
        return randomize ? name + " Min" : name;
    }

    public void ApplyMin()
    {
        //value = Mathf.Max(min, value);
        max = Mathf.Max(value, max);
    }

    public float GetValue(bool resetRandomize = true)
    {
        if (resetRandomize)
            wasRandomized = false;
        if (randomize)
        {
            if (!wasRandomized)
            {
                randomizedValue = Random.Range(value, max);
                wasRandomized = true;
            }
            return randomizedValue;
        }
        else return value;
    }

    float GetMax()
    {
        if (randomize) return max;
        else return value;
    }

    public void SetValue(float value)
    {
        this.value = value;
        randomize = false;
    }

    public static implicit operator float(Randomizable obj) => obj.GetValue(false);
}

#if UNITY_EDITOR
#if ODIN_INSPECTOR
public class RandomizableAttributeProcessor : AttributeToChildrenProcessor<Randomizable>
{
    public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (member.Name == "value")
        {
            Randomizable myProp = (parentProperty.ValueEntry as IPropertyValueEntry<Randomizable>).SmartValue;
            string label = myProp.name + " Min";
            float width = GetValueLabelSize(label);
            attributes.Add(new LabelWidthAttribute(width));
        }

        base.ProcessChildMemberAttributes(parentProperty, member, attributes);
    }

    protected override string[] InheritableChildren()
    {
        return new string[]
        {
            "value",
            "max"
        };
    }

    float GetValueLabelSize(string text)
    {
        GUIStyle labelStyle = GUI.skin.label;
        return labelStyle.CalcSize(new GUIContent(text)).x;
    }
}
#endif
#endif
