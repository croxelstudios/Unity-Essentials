using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXStringEvent
{
    [SerializeField]
    StringEvent unityEvent = null;
    [Serializable]
    public class StringEvent : UnityEvent<string> { }

    public void Invoke(string arg0)
    {
        unityEvent?.Invoke(arg0);
    }

    public void AddListener(UnityAction<string> call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<string> call)
    {
        unityEvent.RemoveListener(call);
    }
}

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