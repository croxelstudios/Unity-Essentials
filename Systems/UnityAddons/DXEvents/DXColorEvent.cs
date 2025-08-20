using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXColorEvent : DXTypedEvent<Color>
{ }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXColorEvent))]
public class DXColorEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif