using UnityEngine;
using System;
using System.Reflection;
using UnityEditor.AI;

using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class NavMeshAgentTypeSelectorAttribute : PropertyAttribute
{
#if UNITY_EDITOR
    public bool UseDefaultTagFieldDrawer = false;

    public static bool InterpretInEventsDrawer(MethodInfo method, SerializedProperty argument, Rect argRect)
    {
        object[] tagSelectorAttributes = null;
        if (method != null)
            tagSelectorAttributes = method.GetCustomAttributes(typeof(NavMeshAgentTypeSelectorAttribute), true);
        if ((tagSelectorAttributes != null) && (tagSelectorAttributes.Length > 0) &&
            (argument.propertyType == SerializedPropertyType.Integer))
        {
            AgentTypePopup(argument, argRect);
            return true;
        }
        else return false;
    }

    public static void AgentTypePopup(SerializedProperty agentTypeID)
    {
        Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
        AgentTypePopup(agentTypeID, rect);
    }

    public static void AgentTypePopup(SerializedProperty agentTypeID, Rect rect)
    {
        AgentTypePopup(agentTypeID.displayName, agentTypeID, rect);
    }

    public static void AgentTypePopup(string labelName, SerializedProperty agentTypeID)
    {
        Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
        AgentTypePopup(labelName, agentTypeID, rect);
    }

    public static void AgentTypePopup(string labelName, SerializedProperty agentTypeID, Rect rect)
    {
        var index = -1;
        var count = NavMesh.GetSettingsCount();
        var agentTypeNames = new string[count + 2];
        for (var i = 0; i < count; i++)
        {
            var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
            var name = NavMesh.GetSettingsNameFromID(id);
            agentTypeNames[i] = name;
            if (id == agentTypeID.intValue)
                index = i;
        }
        agentTypeNames[count] = "";
        agentTypeNames[count + 1] = "Open Agent Settings...";

        bool validAgentType = index != -1;
        if (!validAgentType)
        {
            EditorGUILayout.HelpBox("Agent Type invalid.", MessageType.Warning);
        }

        EditorGUI.BeginProperty(rect, GUIContent.none, agentTypeID);

        EditorGUI.BeginChangeCheck();
        index = EditorGUI.Popup(rect, labelName, index, agentTypeNames);
        if (EditorGUI.EndChangeCheck())
        {
            if (index >= 0 && index < count)
            {
                int id = NavMesh.GetSettingsByIndex(index).agentTypeID;
                agentTypeID.intValue = id;
            }
            else if (index == count + 1)
            {
                NavMeshEditorHelpers.OpenAgentSettings(-1);
            }
        }

        EditorGUI.EndProperty();
    }
#endif
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(NavMeshAgentTypeSelectorAttribute))]
public class NavMeshAgentTypeSelectorAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) //TO DO: Maybe use this label? could be an argument for DrawTagField
    {
        if (property.propertyType == SerializedPropertyType.Integer)
        {
            NavMeshAgentTypeSelectorAttribute.AgentTypePopup(property, position);
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
#endif