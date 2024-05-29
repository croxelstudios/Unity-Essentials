
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
public class EnumPopupAttribute : PropertyAttribute
{
#if UNITY_EDITOR
    public Type enumType;

    public EnumPopupAttribute(Type enumType)
    {
        this.enumType = enumType;
    }

    public static bool InterpretInEventsDrawer(MethodInfo method, SerializedProperty argument, Rect argRect)
    {
        object[] enumPopupAttributes = null;
        if (method != null) enumPopupAttributes = method.GetCustomAttributes(typeof(EnumPopupAttribute), true);
        if ((enumPopupAttributes != null) && (enumPopupAttributes.Length > 0) &&
            (argument.propertyType == SerializedPropertyType.Integer))
        {
            // Make an enum popup
            var enumType = ((EnumPopupAttribute)enumPopupAttributes[0]).enumType;
            var value = (Enum)Enum.ToObject(enumType, argument.intValue);
            argument.intValue = Convert.ToInt32(EditorGUI.EnumPopup(argRect, value));
            return true;
        }
        else return false;
    }
#endif
}

