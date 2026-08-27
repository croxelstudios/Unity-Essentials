using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
public static class PropertyMenuInterceptor
{
    static MethodInfo fillPropertyContextMenu;

    static PropertyMenuInterceptor()
    {
        fillPropertyContextMenu =
            typeof(EditorGUI).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
        .FirstOrDefault(m =>
        {
            if (m.Name != "FillPropertyContextMenu")
                return false;

            ParameterInfo[] p = m.GetParameters();

            return p.Length == 3 &&
                   p[0].ParameterType == typeof(SerializedProperty) &&
                   p[1].ParameterType == typeof(SerializedProperty) &&
                   p[2].ParameterType == typeof(GenericMenu);
        });

        if (fillPropertyContextMenu == null)
            Debug.LogError("Could not find EditorGUI.FillPropertyContextMenu.");
    }

    /// <summary>
    /// Registers a GUI rect whose context menu should belong
    /// to the specified SerializedProperty.
    /// </summary>
    public static void CheckRightClickArea(Rect rect, SerializedProperty property)
    {
        Event current = Event.current;
        if ((current.type == EventType.MouseUp) &&
            (current.button == 1) && rect.Contains(current.mousePosition))
        {
            GenericMenu contextMenu = new();
            InvokeFillPropertyContextMenu(property, contextMenu);

            contextMenu.DropDown(rect);
            Event.current.Use();
        }
    }

    private static void InvokeFillPropertyContextMenu(SerializedProperty property, GenericMenu menu)
    {
        ParameterInfo[] parameters = fillPropertyContextMenu.GetParameters();

        object[] arguments = new object[parameters.Length];

        int serializedPropertyIndex = 0;

        for (int i = 0; i < parameters.Length; i++)
        {
            Type type = parameters[i].ParameterType;

            if (type == typeof(SerializedProperty))
            {
                // First SerializedProperty = property being built.
                // Additional SerializedProperty parameters (if any)
                // are treated as linkedProperty and passed as null.
                if (serializedPropertyIndex == 0)
                    arguments[i] = property;
                else
                    arguments[i] = null;

                serializedPropertyIndex++;
            }
            else if (type == typeof(GenericMenu))
                arguments[i] = menu;
            else
                // Current Unity versions may have extra parameters
                // such as VisualElement. null is appropriate here.
                arguments[i] = null;
        }

        fillPropertyContextMenu.Invoke(null, arguments);
    }
}
#endif
