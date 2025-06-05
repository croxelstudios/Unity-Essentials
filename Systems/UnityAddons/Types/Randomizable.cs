using UnityEngine;
using Sirenix.OdinInspector;
using System.Reflection;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

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
    [MinValue("@min")]
    float value;
    [SerializeField]
    [HorizontalGroup(GroupName = "high", LabelWidth = 30f)]
    [ShowIf("randomize")]
    [MinValue("@value")]
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
