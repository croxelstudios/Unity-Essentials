using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXTransformEvent : DXTypedEvent<Transform>
{ }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXTransformEvent))]
public class DXTransformEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif