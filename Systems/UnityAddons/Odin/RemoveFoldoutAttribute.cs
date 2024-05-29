using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class RemoveFoldoutAttribute : PropertyAttribute
{
#if UNITY_EDITOR
    public RemoveFoldoutAttribute()
    {
    }

    public static void DrawSubfields(InspectorProperty property)
    {
        PropertyChildren children = property.Children;
        for (int i = 0; i < children.Count; i++)
            children[i].Draw();
    }
#endif
}

#if UNITY_EDITOR
public class RemoveFoldoutAttributeDrawer<T> : OdinAttributeDrawer<RemoveFoldoutAttribute, T>
{
    private InspectorProperty elementsProp;

    protected override void Initialize()
    {
        elementsProp = Property.Children["elements"];
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        InspectorProperty prop = ValueEntry.Property;
        if (ValueEntry.BaseValueType.IsSubclassOf(typeof(BWeightedList)))
        {
            WeightedObjectDrawerHelper.WLProperty = ValueEntry.Property;
            elementsProp.Draw();
        }
        else RemoveFoldoutAttribute.DrawSubfields(prop);
    }
}
#endif
