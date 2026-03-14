using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;


#if UNITY_EDITOR
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif
using UnityEditor;
#endif

public class CentralizedSettings : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    VariableReference[] variables = new VariableReference[1];
    [SerializeField]
    Holder holder = new Holder(new VariableReference[1]);

    void OnValidate()
    {
        holder.variables = variables;
    }

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
#endif
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CentralizedSettings.Holder))]
public class CentralizedSettingsDrawer : PropertyDrawer
{
    const float EXTRASIZE = 5f;
    const string variablesName = "variables";
    const string componentName = "component";
    const string displayName = "displayName";
    const string nameName = "name";
#if ODIN_INSPECTOR
    static Dictionary<Object, PropertyTree> odinTrees;
#endif

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
#if ODIN_INSPECTOR
        return 0f;
#else
        property = ProcessProperty(property, out SerializedProperty[] children);

        float res = EditorGUIUtility.singleLineHeight * 1f;

        for (int i = 0; i < children.Length; i++)
        {
            SerializedProperty prop = GetReferenceValueProperty(children[i], out GUIContent lb);
            if (prop != null)
                res += EditorGUI.GetPropertyHeight(prop, lb, true);
            else res += EditorGUIUtility.singleLineHeight;
            res += EXTRASIZE;
        }
        return res;
#endif
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property = ProcessProperty(property, out SerializedProperty[] children);

        EditorGUI.BeginProperty(position, label, property);

        for (int i = 0; i < children.Length; i++)
        {
            SerializedProperty prop = GetReferenceValueProperty(children[i], out GUIContent lb);
            if (prop != null)
            {
#if ODIN_INSPECTOR
                PropertyTree tree = GetTree(property, prop.serializedObject);
                tree.BeginDraw(true);
                InspectorProperty odinProp = tree.GetPropertyAtUnityPath(prop.propertyPath);
                odinProp.Draw(lb);
                tree.EndDraw();
                tree.ApplyChanges();
#else
                position.height = EditorGUI.GetPropertyHeight(prop, lb, true);
                EditorGUI.PropertyField(position, prop, lb, true);
                prop.serializedObject.ApplyModifiedProperties();
#endif
            }
            else
            {
                position.height = EditorGUIUtility.singleLineHeight;
                if ((lb == null) || (lb.text == ""))
                {
                    string content = "*empty*";
#if ODIN_INSPECTOR
                    SirenixEditorGUI.BeginVerticalPropertyLayout(new GUIContent(content), out position);
                    SirenixEditorGUI.EndVerticalPropertyLayout();
#else
                    EditorGUI.LabelField(position, content);
#endif
                }
                else
                {
#if ODIN_INSPECTOR
                    position = SirenixEditorGUI.BeginVerticalPropertyLayout(GUIContent.none, out Rect labelRect);
                    position.height = EditorGUIUtility.singleLineHeight;
#endif
                    EditorGUI.DropShadowLabel(position, lb);
#if ODIN_INSPECTOR
                    SirenixEditorGUI.EndVerticalPropertyLayout();
#endif
                }
            }
            position.y += position.height + EXTRASIZE;
        }

        EditorGUI.EndProperty();
    }

    SerializedProperty GetReferenceValueProperty(SerializedProperty refProp, out GUIContent label)
    {
        Component comp = refProp.FindPropertyRelative(componentName).objectReferenceValue
            as Component;

        string propName = refProp.FindPropertyRelative(nameName).stringValue;
        string dispName = refProp.FindPropertyRelative(displayName).stringValue;

        if (string.IsNullOrEmpty(dispName))
            dispName = propName.ToDisplayName();
        else dispName = dispName.ToDisplayName();
        label = new GUIContent(dispName);

        if (comp == null)
        {
            if (!string.IsNullOrEmpty(propName))
                label = null;
            return null;
        }

        return comp.GetSerializedProperty(propName);
    }

    SerializedProperty ProcessProperty(SerializedProperty property, out SerializedProperty[] children,
        bool getVariables = true)
    {
        if (getVariables)
            property = property.FindPropertyRelative(variablesName);

        children = new SerializedProperty[property.arraySize];
        for (int i = 0; i < children.Length; i++)
            children[i] = property.GetArrayElementAtIndex(i);

        return property;
    }

#if ODIN_INSPECTOR
    PropertyTree GetTree(SerializedProperty property, SerializedObject obj)
    {
        if (!odinTrees.NotNullContainsKey(obj.targetObject))
        {
            odinTrees = new Dictionary<Object, PropertyTree>();
            property = ProcessProperty(property, out SerializedProperty[] children, false);
            for (int i = 0; i < children.Length; i++)
            {
                SerializedProperty prop = GetReferenceValueProperty(children[i], out GUIContent lb);
                if (prop != null)
                {
                    SerializedObject sObj = prop.serializedObject;
                    odinTrees.TryAdd(sObj.targetObject, PropertyTree.Create(sObj));
                }
            }
            Selection.selectionChanged -= ShouldDisposeTrees;
            Selection.selectionChanged += ShouldDisposeTrees;
        }
        return odinTrees[obj.targetObject];
    }

    static void ShouldDisposeTrees()
    {
        DisposeTrees();
    }

    static void DisposeTrees()
    {
        if (odinTrees != null)
        {
            foreach (var tree in odinTrees.Values)
                tree.Dispose();
            odinTrees = null;
        }
        Selection.selectionChanged -= ShouldDisposeTrees;
    }
#endif
}
#endif
