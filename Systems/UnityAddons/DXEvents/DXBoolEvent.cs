using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXBoolEvent : DXTypedEvent<bool>
{ }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXBoolEvent))]
public class DXBoolEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif