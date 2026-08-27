using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
public abstract class AttributeToChildrenProcessor<T> : OdinAttributeProcessor<T>
{
    static Dictionary<InspectorProperty, List<Attribute>> toChildren;
    static Dictionary<InspectorProperty, string[]>  inheritable;

    public override void ProcessSelfAttributes(InspectorProperty parentProperty, List<Attribute> attributes)
    {
        toChildren = toChildren.CreateIfNull();
        toChildren.TryAdd(parentProperty, new List<Attribute>());
        toChildren[parentProperty] = toChildren[parentProperty].ClearOrCreate();

        inheritable = inheritable.CreateIfNull();
        inheritable.TryAdd(parentProperty, InheritableChildren());

        List<Attribute> att = new List<Attribute>();
        foreach (Type type in InheritableTypes())
            att.Add(FindType(attributes, type));
        DeleteNulls(att);

        if (!att.IsNullOrEmpty())
            foreach (Attribute a in att)
            {
                toChildren[parentProperty].Add(PreprocessAttribute(a));
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
        if ((!toChildren.IsNullOrEmpty()) &&
            inheritable[parentProperty].Contains(member.Name) &&
            CanInherit(member))
            attributes.AddRange(toChildren[parentProperty]);
    }

    protected virtual Type[] InheritableTypes()
    {
        return new Type[]
        {
            typeof(MinAttribute),
            typeof(MinValueAttribute),
            typeof(MaxValueAttribute),
            typeof(RangeAttribute)
        };
    }

    protected virtual string[] InheritableChildren()
    {
        return new string[]
        {
            "value"
        };
    }

    protected virtual Attribute PreprocessAttribute(Attribute original)
    {
        if (original is MinAttribute min) //Apparently Odin can't add Unity attributes dynamically
            return new MinValueAttribute(min.min);
        else return original;
    }

    protected virtual bool CanInherit(MemberInfo info)
    {
        Type t;
        if (info is FieldInfo field)
            t = field.FieldType;
        else if (info is PropertyInfo property)
            t = property.PropertyType;
        else return false;

        return (t == typeof(int)) ||
            (t == typeof(float));
    }
}
#endif
#endif
