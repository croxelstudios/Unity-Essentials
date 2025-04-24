using UnityEditor;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Linq;

public class SignalSettings : MonoBehaviour
{
    [DefaultDrawer]
    [SerializeField]
    BaseSignal[] signals = null;

    public Holder holder = new Holder();

    void OnValidate()
    {
        holder = new Holder(signals);
    }

    [Serializable]
    public struct Holder
    {
        public BaseSignal[] signals;

        public Holder(BaseSignal[] signals)
        {
            this.signals = signals;
        }
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SignalSettings.Holder))]
public class SignalSettingsHolderDrawer : PropertyDrawer
{
    const float SIZEMODIF = 1.2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        property = property.FindPropertyRelative("signals");

        SerializedProperty[] children = new SerializedProperty[property.arraySize];
        for (int i = 0; i < children.Length; i++)
            children[i] = property.GetArrayElementAtIndex(i);

        float res = EditorGUIUtility.singleLineHeight * 1f;

        for (int i = 0; i < children.Length; i++)
        {
            SerializedProperty prop = GetSignalValueProperty(children[i], out GUIContent lb);
            res += EditorGUI.GetPropertyHeight(prop, lb, true) * SIZEMODIF;
        }
        return res;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property = property.FindPropertyRelative("signals");

        SerializedProperty[] children = new SerializedProperty[property.arraySize];
        for (int i = 0; i < children.Length; i++)
            children[i] = property.GetArrayElementAtIndex(i);

        EditorGUI.BeginProperty(position, label, property);

        position.height = EditorGUIUtility.singleLineHeight;
        position.y += EditorGUIUtility.singleLineHeight;

        for (int i = 0; i < children.Length; i++)
        {
            SerializedProperty prop = GetSignalValueProperty(children[i], out GUIContent lb);
            EditorGUI.PropertyField(position, prop, lb);
            prop.serializedObject.ApplyModifiedProperties();
            position.y += EditorGUI.GetPropertyHeight(prop, lb, true) * SIZEMODIF;
        }

        EditorGUI.EndProperty();
    }

    SerializedProperty GetSignalValueProperty(SerializedProperty signalProp, out GUIContent label)
    {
        BaseSignal sig = signalProp.objectReferenceValue as BaseSignal;
        SerializedObject obj = new SerializedObject(sig);
        SerializedProperty prop = obj.FindProperty(
            Application.isPlaying ? "currentValue" : "startValue");

        string[] namePath = sig.name.Split("_");
        string name = namePath[namePath.Length - 1].ToDisplayName();
        label = new GUIContent(name);
        return prop;
    }
}
#endif
