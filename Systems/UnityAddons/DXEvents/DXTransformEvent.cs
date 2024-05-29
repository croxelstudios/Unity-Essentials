using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXTransformEvent
{
    [SerializeField]
    TransformEvent unityEvent = null;
    [Serializable]
    public class TransformEvent : UnityEvent<Transform> { }

    public void Invoke(Transform arg0)
    {
        unityEvent?.Invoke(arg0);
    }

    public void AddListener(UnityAction<Transform> call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<Transform> call)
    {
        unityEvent.RemoveListener(call);
    }
}

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