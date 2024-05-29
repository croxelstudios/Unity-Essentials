using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXIntEvent
{
    [SerializeField]
    IntEvent unityEvent = null;
    [Serializable]
    public class IntEvent : UnityEvent<int> { }

    public void Invoke(int arg0)
    {
        unityEvent?.Invoke(arg0);
    }

    public void AddListener(UnityAction<int> call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<int> call)
    {
        unityEvent.RemoveListener(call);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXIntEvent))]
public class DXIntEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif