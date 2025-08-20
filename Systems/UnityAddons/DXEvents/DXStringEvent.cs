using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXStringEvent : DXTypedEvent<string>
{ }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXStringEvent))]
public class DXStringEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif