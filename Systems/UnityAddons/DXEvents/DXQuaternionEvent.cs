using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DXQuaternionEvent
{
    [SerializeField]
    QuaternionEvent unityEvent = null;
    [Serializable]
    public class QuaternionEvent : UnityEvent<Quaternion> { }

    public void Invoke(Quaternion arg0)
    {
        unityEvent?.Invoke(arg0);
    }

    public void AddListener(UnityAction<Quaternion> call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<Quaternion> call)
    {
        unityEvent.RemoveListener(call);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXQuaternionEvent))]
public class DXQuaternionEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif
