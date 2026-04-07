using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// Mark a method with an integer argument with this to display the argument as an enum popup in the UnityEvent
/// drawer. Use: [EnumAction(typeof(SomeEnumType))]
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class EnumSelectorAttribute : PropertyAttribute, IEventActionAttribute
{
    public Type enumType;

    public EnumSelectorAttribute(Type enumType)
    {
        this.enumType = enumType;
    }

#if UNITY_EDITOR
    public bool InterpretInEventsDrawer(Rect argRect,
        SerializedProperty argument, SerializedProperty listenerTarget)
    {
        if (argument.propertyType == SerializedPropertyType.Integer)
        {
            // Make an enum popup
            Enum value = Enum.ToObject(enumType, argument.intValue) as Enum;
            argument.intValue = Convert.ToInt32(EditorGUI.EnumPopup(argRect, value));
            return true;
        }
        else return false;
    }
#endif
}

