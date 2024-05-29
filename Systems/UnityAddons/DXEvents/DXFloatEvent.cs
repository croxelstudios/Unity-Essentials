using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXFloatEvent
{
    [SerializeField]
    FloatEvent unityEvent = null;
    [Serializable]
    public class FloatEvent : UnityEvent<float> { }

    public void Invoke(float arg0)
    {
        unityEvent?.Invoke(arg0);
    }

    public void AddListener(UnityAction<float> call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<float> call)
    {
        unityEvent.RemoveListener(call);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXFloatEvent))]
public class DXFloatEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif