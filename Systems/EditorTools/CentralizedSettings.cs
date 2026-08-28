using UnityEngine;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine.ProBuilder.MeshOperations;
using System.Linq;



#if UNITY_EDITOR
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif
using UnityEditor;
#endif

[ExecuteAlways]
public class CentralizedSettings : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    protected VariableReference[] variables = new VariableReference[1];
    [SerializeField]
    protected Holder holder = new();

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
    public struct Holder { }
#endif
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CentralizedSettings.Holder))]
public class CentralizedSettingsDrawer : PropertyDrawer
{
    const string variablesName = "variables";
    const string componentName = "component";
    const string displayName = "displayName";
    const string nameName = "name";

    string[] overrideOdin = new[] {
        "m_Enabled",
    };

#if ODIN_INSPECTOR
    static Dictionary<PropertyTreeKey, PropertyTree> odinTrees;
#endif

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property = ProcessProperty(property, out SerializedProperty[] children);

        for (int i = 0; i < children.Length; i++)
        {
            SerializedProperty prop = GetReferenceValueProperty(children[i], out GUIContent lb);
            if (prop != null)
            {
                SerializedObject obj = prop.serializedObject;
#if ODIN_INSPECTOR
                if ((!obj.targetObject.GetType().IsOrInheritsFrom(typeof(MonoBehaviour))) ||
                    overrideOdin.Contains(prop.name))
                    prop.OnGUIChildDraw(lb);
                else
                {
                    RemoveAddAttributes.Remove(
                        typeof(HideLabelAttribute),
                        typeof(LabelTextAttribute),
                        typeof(HorizontalGroupAttribute),
                        typeof(IndentAttribute),
                        typeof(LabelWidthAttribute));
                    {
                        PropertyTree tree = GetTree(obj);
                        {
                            tree.BeginDraw(true);
                            {
                                InspectorProperty odinProp = tree.GetPropertyAtUnityPath(prop.propertyPath);
                                odinProp.Draw(lb);
                            }
                            tree.EndDraw();
                        }
                        tree.ApplyChanges();
                    }
                    RemoveAddAttributes.Restore();
                }
#else
                prop.OnGUIChildDraw(lb);
#endif
            }
            else
            {
                if ((lb == null) || (lb.text == ""))
                {
                    string content = "*empty*";
#if ODIN_INSPECTOR
                    SirenixEditorGUI.BeginVerticalPropertyLayout(new GUIContent(content), out position);
                    SirenixEditorGUI.EndVerticalPropertyLayout();
#else
                    EditorGUILayout.LabelField(content);
#endif
                }
                else
                {
#if ODIN_INSPECTOR
                    position = SirenixEditorGUI.BeginVerticalPropertyLayout(GUIContent.none, out Rect labelRect);
                    position.height = EditorGUIUtility.singleLineHeight;
#endif
                    EditorGUI.DropShadowLabel(EditorGUILayout.GetControlRect(), lb);
#if ODIN_INSPECTOR
                    SirenixEditorGUI.EndVerticalPropertyLayout();
#endif
                }
            }
        }
    }

    SerializedProperty GetReferenceValueProperty(SerializedProperty refProp, out GUIContent label)
    {
        SerializedProperty compProp = refProp.FindPropertyRelative(componentName);
        SerializedProperty nameProp = refProp.FindPropertyRelative(nameName);

        string propName = nameProp.stringValue;

        string dispName = refProp.FindPropertyRelative(displayName).stringValue;

        if (string.IsNullOrEmpty(dispName))
            dispName = propName.ToDisplayName();
        else dispName = dispName.ToDisplayName();
        label = new GUIContent(dispName);

        if (nameProp.hasMultipleDifferentValues)
            return null;

        if (string.IsNullOrEmpty(propName))
            return null;

        if (propName == "enabled") propName = "m_Enabled";

        Object[] comps = new Object[refProp.serializedObject.targetObjects.Length];
        for (int i = 0; i < comps.Length; i++)
        {
            SerializedObject obj =
                new SerializedObject(compProp.serializedObject.targetObjects[i]);

            SerializedProperty prop = obj.FindProperty(refProp.propertyPath);

            comps[i] = prop.FindPropertyRelative(componentName).objectReferenceValue;

            if (comps[i] == null)
            {
                if (!string.IsNullOrEmpty(propName))
                    label = null;
                return null;
            }
        }

        SerializedObject compsObject = new SerializedObject(comps);

        return compsObject.FindProperty(propName);
    }

    SerializedProperty ProcessProperty(SerializedProperty property, out SerializedProperty[] children,
        bool getVariables = true)
    {
        if (getVariables)
        {
            SerializedObject obj = property.serializedObject;
            property = obj.FindProperty(variablesName);
        }

        children = new SerializedProperty[property.minArraySize];
        for (int i = 0; i < children.Length; i++)
            children[i] = property.GetArrayElementAtIndex(i);

        return property;
    }

#if ODIN_INSPECTOR
    PropertyTree GetTree(SerializedObject obj)
    {
        odinTrees = odinTrees.CreateIfNull();
        PropertyTreeKey key = new(obj);
        if (!odinTrees.ContainsKey(key))
        {
            odinTrees.TryAdd(new PropertyTreeKey(obj), PropertyTree.Create(obj));
            Selection.selectionChanged -= ShouldDisposeTrees;
            Selection.selectionChanged += ShouldDisposeTrees;
            AssemblyReloadEvents.beforeAssemblyReload -= DisposeTrees;
            AssemblyReloadEvents.beforeAssemblyReload += DisposeTrees;
            EditorApplication.quitting -= DisposeTrees;
            EditorApplication.quitting += DisposeTrees;
        }
        return odinTrees[key];
    }

    struct PropertyTreeKey : IEquatable<PropertyTreeKey>
    {
        Type t;
        int[] ids;

        public PropertyTreeKey(SerializedObject serializedObject)
        {
            Object[] targets = serializedObject.targetObjects;
            ids = new int[targets.Length];

            t = targets[0].GetType();
            for (int i = 0; i < ids.Length; i++)
                ids[i] = targets[i].GetInstanceID();

            Array.Sort(ids);
        }

        public bool Equals(PropertyTreeKey other)
        {
            if (t != other.t)
                return false;

            return ids.AsSpan().SequenceEqual(other.ids);
        }

        public override bool Equals(object obj)
        {
            return obj is PropertyTreeKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(t);

            for (int i = 0; i < ids.Length; i++)
                hash.Add(ids[i]);

            return hash.ToHashCode();
        }
    }

    static void ShouldDisposeTrees()
    {
        DisposeTrees();
    }

    static void DisposeTrees()
    {
        if (odinTrees != null)
        {
            foreach (PropertyTree tree in odinTrees.Values)
                tree.Dispose();
            odinTrees = null;
        }
        Selection.selectionChanged -= ShouldDisposeTrees;
        AssemblyReloadEvents.beforeAssemblyReload -= DisposeTrees;
        EditorApplication.quitting -= DisposeTrees;
    }
#endif
}
#endif
