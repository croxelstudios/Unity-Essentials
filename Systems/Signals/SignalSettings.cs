using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "SignalSettings", menuName = "Croxel Scriptables/SignalSettings")]
public class SignalSettings : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField]
    [PropertyOrder(-1)]
    string substring = "";

    [DefaultDrawer]
    [SerializeField]
    BaseSignal[] signals = null;
#endif

    public Holder holder = new Holder();

#if UNITY_EDITOR
    void OnValidate()
    {
        holder = new Holder(signals, substring);
    }

    [Button]
    [PropertyOrder(-1)]
    public void SearchSubstring()
    {
        if (!string.IsNullOrEmpty(substring))
        {
            List<BaseSignal> list = new List<BaseSignal>();
            list.AddRange(signals);
            for (int i = list.Count - 1; i >= 0; i--)
                if ((list[i] == null) || !list[i].name.Contains(substring))
                    list.RemoveAt(i);

            BaseSignal[] sig = BaseSignal.GetFromSubstring<BaseSignal>(substring);
            for (int i = 0; i < sig.Length; i++)
                if (!list.Contains(sig[i]))
                    list.Add(sig[i]);

            signals = list.ToArray();
            holder = new Holder(signals, substring);
        }
    }
#endif

    [Serializable]
    public struct Holder
    {
        public BaseSignal[] signals;
        public string substring;

        public Holder(BaseSignal[] signals)
        {
            this.signals = signals;
            substring = "";
        }

        public Holder(BaseSignal[] signals, string substring)
        {
            this.signals = signals;
            this.substring = substring;
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
            if (prop != null)
                res += EditorGUI.GetPropertyHeight(prop, lb, true) * SIZEMODIF;
        }
        return res;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string substring = property.FindPropertyRelative("substring").stringValue;
        property = property.FindPropertyRelative("signals");

        SerializedProperty[] children = new SerializedProperty[property.arraySize];
        for (int i = 0; i < children.Length; i++)
            children[i] = property.GetArrayElementAtIndex(i);

        EditorGUI.BeginProperty(position, label, property);

        position.height = EditorGUIUtility.singleLineHeight;
        position.y += EditorGUIUtility.singleLineHeight;

        for (int i = 0; i < children.Length; i++)
        {
            SerializedProperty prop = GetSignalValueProperty(children[i], out GUIContent lb, substring);
            if (prop != null)
            {
                EditorGUI.PropertyField(position, prop, lb);
                prop.serializedObject.ApplyModifiedProperties();
                position.y += EditorGUI.GetPropertyHeight(prop, lb, true) * SIZEMODIF;
            }
        }

        EditorGUI.EndProperty();
    }

    SerializedProperty GetSignalValueProperty(SerializedProperty signalProp, out GUIContent label, string substring = "")
    {
        BaseSignal sig = signalProp.objectReferenceValue as BaseSignal;
        if (sig == null)
        {
            label = new GUIContent("");
            return null;
        }
        SerializedObject obj = new SerializedObject(sig);
        SerializedProperty prop = obj.FindProperty(
            Application.isPlaying ? "currentValue" : "startValue");

        string name = sig.name;
        if (!string.IsNullOrEmpty(substring))
            name = name.Split(substring).Last().Replace("_", " ");
        else name = name.Split("_").Last();
        name = name.ToDisplayName();
        label = new GUIContent(name);
        return prop;
    }
}
#endif
