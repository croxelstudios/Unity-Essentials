using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class SortingLayerSelectorAttribute : PropertyAttribute
{
#if UNITY_EDITOR
    public bool UseDefaultTagFieldDrawer = false;

    public static bool InterpretInEventsDrawer(MethodInfo method, SerializedProperty argument, Rect argRect)
    {
        object[] sortingLayerSelectorAttributes = null;
        if (method != null) sortingLayerSelectorAttributes =
                method.GetCustomAttributes(typeof(SortingLayerSelectorAttribute), true);
        if ((sortingLayerSelectorAttributes != null) && (sortingLayerSelectorAttributes.Length > 0) &&
            (argument.propertyType == SerializedPropertyType.String))
        {
            SortingLayerSelectorAttribute attrib = (SortingLayerSelectorAttribute)sortingLayerSelectorAttributes[0];
            DrawSortingLayerField(attrib, argument, argRect);
            return true;
        }
        else return false;
    }

    public static void DrawSortingLayerField(SortingLayerSelectorAttribute attrib, SerializedProperty property, Rect argRect, bool drawWithLabel = false)
    {
        if (attrib.UseDefaultTagFieldDrawer)
        {
            if (drawWithLabel)
                property.stringValue = EditorGUI.TagField(argRect, property.displayName, property.stringValue);
            else property.stringValue = EditorGUI.TagField(argRect, property.stringValue);
        }
        else
        {
            //generate the taglist + custom tags
            List<string> sortingLayersList = new List<string>();
            sortingLayersList.Add("<NoSortingLayer>");
            sortingLayersList.AddRange(SortingLayer.layers.Select(x => x.name));
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
                    if (sortingLayersList[i] == propertyString)
                    {
                        index = i;
                        break;
                    }
                }
            }

            string[] sortingLayersArray = sortingLayersList.ToArray();

            //Draw the popup box with the current selected index
            if (drawWithLabel)
                index = EditorGUI.Popup(argRect, property.displayName, index, sortingLayersArray);
            else index = EditorGUI.Popup(argRect, index, sortingLayersArray);

            //Adjust the actual string value of the property based on the selection
            if (index == 0)
            {
                property.stringValue = "";
            }
            else if (index >= 1)
            {
                property.stringValue = sortingLayersArray[index];
            }
            else
            {
                property.stringValue = "";
            }
        }
    }
#endif
}

#if UNITY_EDITOR
//Original by DYLAN ENGELMAN http://jupiterlighthousestudio.com/custom-inspectors-unity/
//Altered by Brecht Lecluyse http://www.brechtos.com
//Altered by HypercubeCore to add event calls support
[CustomPropertyDrawer(typeof(SortingLayerSelectorAttribute))]
public class SortingLayerSelectorAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) //TO DO: Maybe use this label? could be an argument for DrawTagField
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);

            var attrib = attribute as SortingLayerSelectorAttribute;

            SortingLayerSelectorAttribute.DrawSortingLayerField(attrib, property, position, true);

            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
#endif