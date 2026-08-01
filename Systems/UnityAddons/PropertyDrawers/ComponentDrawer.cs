#if UNITY_EDITOR
using Mono.CSharp;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(Component))]
public class ComponentDrawer : PropertyDrawer
{
    static Dictionary<string, Object> selectedSources;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Type type = FieldType();
        SerializedObject obj = property.serializedObject;
        if (type.DisallowsMultiple())
        {
            EditorGUI.PropertyField(position, property, label);
            obj.ApplyModifiedProperties();
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        Rect valueRect = EditorGUI.PrefixLabel(position, label);

        // Don't inherit indentation
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Two halves
        Rect leftRect = valueRect;
        leftRect.width = (valueRect.width - 2) * 0.5f;

        Rect rightRect = valueRect;
        rightRect.x = leftRect.xMax + 2;
        rightRect.width = leftRect.width;

        // Game Object
        selectedSources = selectedSources.CreateIfNull();
        string key = property.serializedObject.targetObject.GetInstanceID() +
            "/" + property.propertyPath;
        Component component = property.objectReferenceValue as Component;
        Object oldSource;
        if (component != null)
        {
            oldSource = component.gameObject;
            selectedSources.Set(key, oldSource);
        }
        else selectedSources.TryGetValue(key, out oldSource);
        bool redoFoldout = false;
        EditorGUI.BeginChangeCheck();
        Object newSource = EditorGUI.ObjectField(
            leftRect, GUIContent.none, oldSource, typeof(Object), true);
        if (EditorGUI.EndChangeCheck())
        {
            if (newSource == null)
            {
                property.objectReferenceValue = null;
                obj.ApplyModifiedProperties();
            }
            else if (newSource is Component newComponent)
            {
                newSource = newComponent.gameObject;
                if (newComponent.GetType().IsOrInheritsFrom(type))
                {
                    property.objectReferenceValue = newComponent;
                    obj.ApplyModifiedProperties();
                }
            }
            else redoFoldout = true;
            selectedSources.Set(key, newSource);
        }

        // Component foldout
        if (redoFoldout && (newSource != null))
        {
            if (property.objectReferenceValue == null)
                property.objectReferenceValue = newSource.GetComponent(type);
            else if (oldSource != null)
            {
                // Attempt to maintain the function pointer and
                // component pointer if someone changes
                // the target object and it has the correct component type on it.

                GameObject oldSrc = (GameObject)oldSource;
                GameObject newSrc = (GameObject)newSource;

                Type refType = property.objectReferenceValue.GetType();

                Component[] oldComponentList = oldSrc.GetComponents(refType);

                int componentLocationOffset = 0;
                for (int i = 0; i < oldComponentList.Length; ++i)
                {
                    if (oldComponentList[i] == property.objectReferenceValue)
                        break;

                    // Only take exact matches for component type
                    if (oldComponentList[i].GetType() == refType)
                        componentLocationOffset++;
                }

                Component[] newComponentList = newSrc.GetComponents(refType);

                int newComponentIndex = 0;
                int componentCount = -1;
                for (int i = 0; i < newComponentList.Length; ++i)
                {
                    if (componentCount == componentLocationOffset)
                        break;

                    if (newComponentList[i].GetType() == refType)
                    {
                        newComponentIndex = i;
                        componentCount++;
                    }
                }

                if ((newComponentList.Length > 0) &&
                    (newComponentList[newComponentIndex].GetType() == refType))
                    property.objectReferenceValue = newComponentList[newComponentIndex];
                else property.objectReferenceValue = newSource.GetComponent(type);
            }
            
            obj.ApplyModifiedProperties();
        }
        EditorGUI.BeginDisabledGroup(newSource == null);
        {
            GUIContent buttonContent;

            if (EditorGUI.showMixedValue)
                buttonContent = new GUIContent("\u2014", "Mixed Values");
            else
            {
                if (property.objectReferenceValue == null)
                    buttonContent = new GUIContent("None");
                else buttonContent = new GUIContent(GetComponentDisplayName(property));
            }
            if (GUI.Button(rightRect, buttonContent, EditorStyles.popup))
                BuildPopupMenu(newSource, property).DropDown(rightRect);
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    class ComponentTypeCount
    {
        public int TotalCount = 0;
        public int CurrentCount = 1;
    }

    class MenuData
    {
        public SerializedObject serializedObject;
        public string propertyPath;
        public Component component;
        SerializedProperty _property;
        public SerializedProperty property
        {
            get
            {
                if (_property == null)
                    _property = serializedObject.FindProperty(propertyPath);
                return _property;
            }
        }

        public MenuData(SerializedObject serializedObject, string propertyPath, Component component)
        {
            this.serializedObject = serializedObject;
            this.propertyPath = propertyPath;
            this.component = component;
        }

        public MenuData(SerializedProperty property, Component component)
        {
            serializedObject = property.serializedObject;
            propertyPath = property.propertyPath;
            _property = property;
            this.component = component;
        }
    }

    GenericMenu BuildPopupMenu(Object targetObj, SerializedProperty componentProp)
    {
        Type type = FieldType();
        GenericMenu menu = new();

        menu.AddItem(new GUIContent("None"), componentProp.objectReferenceValue == null,
            ClearEventFunctionCallback, componentProp);
        menu.AddSeparator("");

        if (targetObj is Component)
        {
            targetObj = (targetObj as Component).gameObject;
        }
        else if (!(targetObj is GameObject))
            return menu;

        Component[] components = (targetObj as GameObject).GetComponents<Component>();
        Dictionary<Type, ComponentTypeCount> componentTypeCounts =
            new Dictionary<Type, ComponentTypeCount>();

        // We need to know if there are multiple components of a given type before
        // we start going through the components since we only need numbers on
        // component types with multiple instances.
        foreach (Component component in components)
            if (component.GetType().IsOrInheritsFrom(type))
            {
                if (!componentTypeCounts.TryGetValue(
                component.GetType(), out ComponentTypeCount typeCount))
                {
                    typeCount = new ComponentTypeCount();
                    componentTypeCounts.Add(component.GetType(), typeCount);
                }

                typeCount.TotalCount++;
            }

        foreach (Component component in components)
            if (component.GetType().IsOrInheritsFrom(type))
            {
                int componentCount = 0;

                ComponentTypeCount typeCount = componentTypeCounts[component.GetType()];
                if (typeCount.TotalCount > 1)
                    componentCount = typeCount.CurrentCount++;

                AddItem(component, componentProp, menu, componentCount);
            }

        return menu;
    }

    void AddItem(Component component, SerializedProperty componentProp, GenericMenu menu, int componentCount = 0)
    {
        string contentPath = component.GetType().Name +
            (componentCount > 0 ? string.Format("({0})", componentCount) : "");

        bool selected = componentProp.objectReferenceValue == component;
        menu.AddItem(new GUIContent(contentPath), selected, SetComponentCallback,
            new MenuData(componentProp, component));
    }

    // Where the event data actually gets added when you choose a function
    static void SetComponentCallback(object data)
    {
        MenuData menuData = data as MenuData;
        Component comp = menuData.component;
        SerializedProperty prop = menuData.property;
        // Never going to be the case
        //if (comp.GetType().IsOrInheritsFrom(FieldType()))
        {
            prop.objectReferenceValue = comp;
            menuData.serializedObject.ApplyModifiedProperties();
        }
    }

    static void ClearEventFunctionCallback(object componentProp)
    {
        if (componentProp is SerializedProperty sProp)
        {
            sProp.objectReferenceValue = null;
            sProp.serializedObject.ApplyModifiedProperties();
        }
    }

    string GetComponentDisplayName(SerializedProperty componentProp)
    {
        string name = "None";

        if (componentProp.objectReferenceValue == null)
            return name;

        name = componentProp.objectReferenceValue.GetType().Name;
        Component objectComponent = componentProp.objectReferenceValue as Component;

        if (objectComponent != null)
        {
            Type objectType = componentProp.objectReferenceValue.GetType();

            Component[] components = objectComponent.GetComponents(objectType);

            if (components.Length > 1)
            {
                int componentID = 0;
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == objectComponent)
                    {
                        componentID = i + 1;
                        break;
                    }
                }

                name += string.Format("({0})", componentID);
            }
        }

        return name;
    }

    Type FieldType()
    {
        Type type = fieldInfo.FieldType;
        if (type.IsArray)
            type = type.GetElementType();
        return type;
    }
}
#endif
