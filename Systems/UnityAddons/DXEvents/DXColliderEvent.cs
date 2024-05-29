using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXColliderEvent
{
    [SerializeField]
    NDColliderEvent unityEvent = null;
    [Serializable]
    public class NDColliderEvent : UnityEvent<NDCollider> { }

    public void Invoke(NDCollider arg0)
    {
        unityEvent?.Invoke(arg0);
    }

    public void AddListener(UnityAction<NDCollider> call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<NDCollider> call)
    {
        unityEvent.RemoveListener(call);
    }
}

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