using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXGameObjectEvent : DXTypedEvent<GameObject>
{ }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXGameObjectEvent))]
public class DXGameObjectEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif