using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXColliderEvent : DXTypedEvent<NDCollider>
{ }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXColliderEvent))]
public class DXColliderEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif