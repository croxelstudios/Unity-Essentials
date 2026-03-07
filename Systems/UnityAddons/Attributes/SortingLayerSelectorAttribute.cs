using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class SortingLayerSelectorAttribute : PropertyAttribute, IEventActionAttribute
{
#if UNITY_EDITOR
    public bool InterpretInEventsDrawer(Rect argRect,
        SerializedProperty argument, SerializedProperty listenerTarget)
    {
        if (argument.propertyType == SerializedPropertyType.String)
        {
            DrawSortingLayerField(argument, argRect);
            return true;
        }
        else return false;
    }

    public void DrawSortingLayerField(SerializedProperty property, Rect argRect, bool drawWithLabel = false, GUIContent label = null)
    {
        //generate the taglist + custom tags
        List<GUIContent> sortingLayersList = new List<GUIContent>
            { new GUIContent("<NoSortingLayer>") };
        sortingLayersList.AddRange(SortingLayer.layers.Select(x => new GUIContent(x.name)));
        string propertyString = property.stringValue;
        int index = -1;
        if (propertyString == "")
        {
            //The tag is empty
            index = 0; //first index is the special <notag> entry
        }
        else
        {
            //check if there is an entry that matches the entry and get the index
            //we skip index 0 as that is a special custom case
            for (int i = 1; i < sortingLayersList.Count; i++)
            {
                if (sortingLayersList[i].text == propertyString)
                {
                    index = i;
                    break;
                }
            }
        }

        GUIContent[] sortingLayersArray = sortingLayersList.ToArray();

        //Draw the popup box with the current selected index
        if (drawWithLabel)
            index = EditorGUI.Popup(argRect,
                label == null ? new GUIContent(property.displayName) : label,
                index, sortingLayersArray);
        else index = EditorGUI.Popup(argRect, index, sortingLayersArray);

        //Adjust the actual string value of the property based on the selection
        if (index == 0)
        {
            property.stringValue = "";
        }
        else if (index >= 1)
        {
            property.stringValue = sortingLayersArray[index].text;
        }
        else
        {
            property.stringValue = "";
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SortingLayerSelectorAttribute))]
public class SortingLayerSelectorAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);

            var attrib = attribute as SortingLayerSelectorAttribute;

            attrib.DrawSortingLayerField(property, position, true, label);

            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
#endif