using UnityEngine;
using UnityEditor;
using System;

public class CentralizedSettings : MonoBehaviour
{
    [SerializeField]
    VariableReference[] variables = new VariableReference[1];
    [SerializeField]
    Holder holder = new Holder(new VariableReference[1]);

#if UNITY_EDITOR
    void OnValidate()
    {
        holder.variables = variables;
    }
#endif

    [Serializable]
    public struct VariableReference
    {
        public string displayName;
        public Component component;
        public string name;

        public VariableReference(string displayName, Component component, string name)
        {
            this.displayName = displayName;
            this.component = component;
            this.name = name;
        }
    }

    [Serializable]
    public struct Holder
    {
        public VariableReference[] variables;

        public Holder(VariableReference[] variables)
        {
            this.variables = variables;
        }
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CentralizedSettings.Holder))]
public class CentralizedSettingsDrawer : PropertyDrawer
{
    const float EXTRASIZE = 10f;
    const string variablesName = "variables";
    const string componentName = "component";
    const string displayName = "displayName";
    const string nameName = "name";

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        property = property.FindPropertyRelative(variablesName);

        SerializedProperty[] children = new SerializedProperty[property.arraySize];
        for (int i = 0; i < children.Length; i++)
            children[i] = property.GetArrayElementAtIndex(i);

        float res = EditorGUIUtility.singleLineHeight * 1f;

        for (int i = 0; i < children.Length; i++)
        {
            SerializedProperty prop = GetReferenceValueProperty(children[i], out GUIContent lb);
            if (prop != null)
                res += EditorGUI.GetPropertyHeight(prop, lb, true) + EXTRASIZE;
        }
        return res;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property = property.FindPropertyRelative(variablesName);

        SerializedProperty[] children = new SerializedProperty[property.arraySize];
        for (int i = 0; i < children.Length; i++)
            children[i] = property.GetArrayElementAtIndex(i);

        EditorGUI.BeginProperty(position, label, property);

        position.y += EditorGUIUtility.singleLineHeight;

        for (int i = 0; i < children.Length; i++)
        {
            SerializedProperty prop = GetReferenceValueProperty(children[i], out GUIContent lb);
            if (prop != null)
            {
                position.height = EditorGUI.GetPropertyHeight(prop, lb, true);
                EditorGUI.PropertyField(position, prop, lb, true);
                prop.serializedObject.ApplyModifiedProperties();
                position.y += position.height + EXTRASIZE;
            }
        }

        EditorGUI.EndProperty();
    }

    SerializedProperty GetReferenceValueProperty(SerializedProperty refProp, out GUIContent label)
    {
        Component comp = refProp.FindPropertyRelative(componentName).objectReferenceValue
            as Component;
        if (comp == null)
        {
            label = new GUIContent("");
            return null;
        }
        SerializedObject obj = new SerializedObject(comp);
        //TO DO: Support sub-properties and arrays
        string propName = refProp.FindPropertyRelative(nameName).stringValue;
        SerializedProperty prop = obj.FindProperty(propName);
        string dispName = refProp.FindPropertyRelative(displayName).stringValue;

        if (string.IsNullOrEmpty(dispName))
            dispName = propName.ToDisplayName();
        else dispName = dispName.ToDisplayName();
        label = new GUIContent(dispName);
        return prop;
    }
}
#endif
