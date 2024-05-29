using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXBoolEvent
{
    [SerializeField]
    BoolEvent unityEvent = null;
    [Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public void Invoke(bool arg0)
    {
        unityEvent?.Invoke(arg0);
    }

    public void AddListener(UnityAction<bool> call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<bool> call)
    {
        unityEvent.RemoveListener(call);
    }
}

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