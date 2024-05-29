using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXSceneEvent
{
    [SerializeField]
    SceneEvent unityEvent = null;
    [Serializable]
    public class SceneEvent : UnityEvent<Scene> { }

    public void Invoke(Scene arg0)
    {
        unityEvent?.Invoke(arg0);
    }

    public void AddListener(UnityAction<Scene> call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<Scene> call)
    {
        unityEvent.RemoveListener(call);
    }
}

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