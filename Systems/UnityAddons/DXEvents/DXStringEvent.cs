using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXStringEvent : DXTypedEvent<string>
{
    [SerializeField]
    protected EventType[] types = new EventType[] { EventType.String };
    [SerializeField]
    UnityEvent isEmpty = null;
    [SerializeField]
    UnityEvent isNotEmpty = null;

    public DXStringEvent() { types = new EventType[] { EventType.String }; }

    public enum EventType
    {
        String, IsEmpty, IsNotEmpty
    }

    public override void Invoke(string arg0)
    {
        bool hasString = false;
        bool hasEmpty = false;
        bool hasNotEmpty = false;
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case EventType.String:
                    hasString = true;
                    break;
                case EventType.IsEmpty:
                    hasEmpty = true;
                    break;
                case EventType.IsNotEmpty:
                    hasNotEmpty = true;
                    break;
            }
        }

        if (hasString)
            unityEvent?.Invoke(arg0);
        if (arg0.IsNullOrEmpty())
        {
            if (hasEmpty)
                isEmpty?.Invoke();
        }
        else if (hasNotEmpty)
            isNotEmpty?.Invoke();
    }

    public void AddIsEmptyListener(UnityAction call)
    {
        isEmpty.AddListener(call);
    }

    public void AddIsNotEmptyListener(UnityAction call)
    {
        isNotEmpty.AddListener(call);
    }

    public void RemoveIsEmptyListener(UnityAction call)
    {
        isEmpty.RemoveListener(call);
    }

    public void RemoveIsNotEmptyListener(UnityAction call)
    {
        isNotEmpty.RemoveListener(call);
    }

    public override bool IsNull()
    {
        bool isNull = true;
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case EventType.String:
                    if ((unityEvent != null) && (unityEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.IsEmpty:
                    if ((isEmpty != null) && (isEmpty.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.IsNotEmpty:
                    if ((isNotEmpty != null) && (isNotEmpty.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
            }
            if (!isNull)
                break;
        }
        return isNull;
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXStringEvent))]
public class DXStringEventDrawer : DXDrawerBase
{
    const string isEmptyName = "isEmpty";
    const string isNotEmptyName = "isNotEmpty";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DrawComplexDXEvent(position, property, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return GetComplexDXEventHeight(property, label);
    }

    protected override void DrawEventType(ref Rect eventRect, SerializedProperty property, SerializedProperty eventType)
    {
        switch (eventType.enumValueIndex)
        {
            case (int)DXVectorEvent.EventType.Vector3:
                eventRect = DrawSubEvent(eventRect, property, unityEventName, eventType);
                break;
            case (int)DXVectorEvent.EventType.Vector2:
                eventRect = DrawSubEvent(eventRect, property, isEmptyName, eventType);
                break;
            case (int)DXVectorEvent.EventType.Vector2XZ:
                eventRect = DrawSubEvent(eventRect, property, isNotEmptyName, eventType);
                break;
        }
    }

    protected override void GetEventTypeHeight(ref float height,
        SerializedProperty property, SerializedProperty eventType)
    {
        switch (eventType.enumValueIndex)
        {
            case (int)DXVectorEvent.EventType.Vector3:
                height += GetHeightOfEvent(property, unityEventName);
                break;
            case (int)DXVectorEvent.EventType.Vector2:
                height += GetHeightOfEvent(property, isEmptyName);
                break;
            case (int)DXVectorEvent.EventType.Vector2XZ:
                height += GetHeightOfEvent(property, isNotEmptyName);
                break;
        }
    }
}
#endif