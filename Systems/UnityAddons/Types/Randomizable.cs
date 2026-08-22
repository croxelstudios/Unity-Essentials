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
public class RandomizableAttributeProcessor : OdinAttributeProcessor<Randomizable>
{
    Dictionary<InspectorProperty, List<Attribute>> toChildren;

    public override void ProcessSelfAttributes(InspectorProperty parentProperty, List<Attribute> attributes)
    {
        toChildren = toChildren.CreateIfNull();
        toChildren.TryAdd(parentProperty, new List<Attribute>());
        toChildren[parentProperty] = toChildren[parentProperty].ClearOrCreate();

        List<Attribute> att = new List<Attribute>();
        att.Add(FindType(attributes, typeof(MinAttribute)));
        att.Add(FindType(attributes, typeof(MinValueAttribute)));
        att.Add(FindType(attributes, typeof(MaxValueAttribute)));
        att.Add(FindType(attributes, typeof(RangeAttribute)));
        DeleteNulls(att);

        if (!att.IsNullOrEmpty())
            foreach (Attribute a in att)
            {
                Attribute add;
                if (a is MinAttribute min) //Apparently Odin can't add Unity attributes dynamically
                    add = new MinValueAttribute(min.min);
                else add = a;
                toChildren[parentProperty].Add(add);
                attributes.Remove(a);
            }
    }

    Attribute FindType(List<Attribute> attributes, Type type)
    {
        return attributes.Find(a => a.GetType() == type);
    }

    void DeleteNulls(List<Attribute> attributes)
    {
        if (!attributes.IsNullOrEmpty())
            for (int i = attributes.Count - 1; i >= 0; i--)
            {
                if (attributes[i] == null)
                    attributes.RemoveAt(i);
            }
    }

    public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (member.Name == "value")
        {
            Randomizable myProp = (parentProperty.ValueEntry as IPropertyValueEntry<Randomizable>).SmartValue;
            string label = myProp.name + " Min";
            float width = GetValueLabelSize(label);
            attributes.Add(new LabelWidthAttribute(width));
            if (!toChildren.IsNullOrEmpty())
                attributes.AddRange(toChildren[parentProperty]);
        }

        if (member.Name == "max")
        {
            if (!toChildren.IsNullOrEmpty())
                attributes.AddRange(toChildren[parentProperty]);
        }
    }

    public float GetValueLabelSize(string text)
    {
        GUIStyle labelStyle = GUI.skin.label;
        return labelStyle.CalcSize(new GUIContent(text)).x;
    }
}
#endif
