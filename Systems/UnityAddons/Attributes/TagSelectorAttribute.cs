using UnityEngine;
using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class TagSelectorAttribute : PropertyAttribute
{
#if UNITY_EDITOR
    public bool UseDefaultTagFieldDrawer = false;
    public string noTagName = "";

    public TagSelectorAttribute(string noTagName = "")
    {
        this.noTagName = noTagName;
    }

    public static bool InterpretInEventsDrawer(MethodInfo method, SerializedProperty argument, Rect argRect)
    {
        object[] tagSelectorAttributes = null;
        if (method != null) tagSelectorAttributes = method.GetCustomAttributes(typeof(TagSelectorAttribute), true);
        if ((tagSelectorAttributes != null) && (tagSelectorAttributes.Length > 0) &&
            (argument.propertyType == SerializedPropertyType.String))
        {
            TagSelectorAttribute attrib = (TagSelectorAttribute)tagSelectorAttributes[0];
            DrawTagField(attrib, argument, argRect);
            return true;
        }
        else return false;
    }

    public static void DrawTagField(TagSelectorAttribute attrib, SerializedProperty property, Rect argRect, bool drawWithLabel = false)
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
            List<string> tagList = new List<string>();
            if (attrib.noTagName == "") tagList.Add("<NoTag>");
            else tagList.Add("<" + attrib.noTagName + ">");
            tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);
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
                for (int i = 1; i < tagList.Count; i++)
                {
                    if (tagList[i] == propertyString)
                    {
                        index = i;
                        break;
                    }
                }
            }

            string[] tagArray = tagList.ToArray();

            //Draw the popup box with the current selected index
            if (drawWithLabel)
                index = EditorGUI.Popup(argRect, property.displayName, index, tagArray);
            else index = EditorGUI.Popup(argRect, index, tagArray);

            //Adjust the actual string value of the property based on the selection
            if (index == 0)
            {
                property.stringValue = "";
            }
            else if (index >= 1)
            {
                property.stringValue = tagArray[index];
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
[CustomPropertyDrawer(typeof(TagSelectorAttribute))]
public class TagSelectorAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) //TO DO: Maybe use this label? could be an argument for DrawTagField
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);

            var attrib = attribute as TagSelectorAttribute;

            TagSelectorAttribute.DrawTagField(attrib, property, position, true);

            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
#endif