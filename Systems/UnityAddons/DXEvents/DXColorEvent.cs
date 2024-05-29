using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXColorEvent
{
    [SerializeField]
    ColorEvent unityEvent = null;
    [Serializable]
    public class ColorEvent : UnityEvent<Color> { }

    public void Invoke(Color arg0)
    {
        unityEvent?.Invoke(arg0);
    }

    public void AddListener(UnityAction<Color> call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<Color> call)
    {
        unityEvent.RemoveListener(call);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXColorEvent))]
public class DXColorEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif