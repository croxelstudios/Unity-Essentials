using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXSceneEvent : DXTypedEvent<Scene>
{ }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXSceneEvent))]
public class DXSceneEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif