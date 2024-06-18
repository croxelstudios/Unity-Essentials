using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXGameObjectEvent
{
    [SerializeField]
    GameObjectEvent unityEvent = null;
    [Serializable]
    public class GameObjectEvent : UnityEvent<GameObject> { }

    public void Invoke(GameObject arg0)
    {
        unityEvent?.Invoke(arg0);
    }

    public void AddListener(UnityAction<GameObject> call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<GameObject> call)
    {
        unityEvent.RemoveListener(call);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXGameObjectEvent))]
public class DXGameObjectEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif