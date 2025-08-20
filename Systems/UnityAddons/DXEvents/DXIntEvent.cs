using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXIntEvent : DXTypedEvent<int>
{
    [SerializeField]
    protected EventType[] types = new EventType[] { EventType.Int };
    [SerializeField]
    UnityEvent<int> absEvent = null;
    [SerializeField]
    UnityEvent<int> negativeEvent = null;

    public DXIntEvent() { types = new EventType[] { EventType.Int }; }

    public enum EventType
    {
        Int, Abs, Negative
    }

    public override void Invoke(int arg0)
    {
        bool hasFloat = false;
        bool hasAbs = false;
        bool hasNegative = false;
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case EventType.Int:
                    hasFloat = true;
                    break;
                case EventType.Abs:
                    hasAbs = true;
                    break;
                case EventType.Negative:
                    hasNegative = true;
                    break;
            }
        }
        if (hasFloat)
            unityEvent?.Invoke(arg0);
        if (hasAbs)
            absEvent?.Invoke(Mathf.Abs(arg0));
        if (hasNegative)
            negativeEvent?.Invoke(-arg0);
    }

    public void AddListener_Abs(UnityAction<int> call)
    {
        absEvent.AddListener(call);
    }

    public void RemoveListener_Abs(UnityAction<int> call)
    {
        absEvent.RemoveListener(call);
    }

    public void AddListener_Negative(UnityAction<int> call)
    {
        negativeEvent.AddListener(call);
    }

    public void RemoveListener_Negative(UnityAction<int> call)
    {
        negativeEvent.RemoveListener(call);
    }

    public override bool IsNull()
    {
        bool isNull = true;
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case EventType.Int:
                    if ((unityEvent != null) && (unityEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.Abs:
                    if ((absEvent != null) || (absEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.Negative:
                    if ((negativeEvent != null) || (negativeEvent.GetPersistentEventCount() > 0))
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

[CustomPropertyDrawer(typeof(DXIntEvent))]
public class DXIntEventDrawer : DXDrawerBase
{
    const string absEventName = "absEvent";
    const string negativeEventName = "negativeEvent";

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
            case (int)DXIntEvent.EventType.Int:
                eventRect = DrawSubEvent(eventRect, property, unityEventName, eventType);
                break;
            case (int)DXIntEvent.EventType.Abs:
                eventRect = DrawSubEvent(eventRect, property, absEventName, eventType);
                break;
            case (int)DXIntEvent.EventType.Negative:
                eventRect = DrawSubEvent(eventRect, property, negativeEventName, eventType);
                break;
        }
    }

    protected override void GetEventTypeHeight(ref float height,
        SerializedProperty property, SerializedProperty eventType)
    {
        switch (eventType.enumValueIndex)
        {
            case (int)DXIntEvent.EventType.Int:
                height += GetHeightOfEvent(property, unityEventName);
                break;
            case (int)DXIntEvent.EventType.Abs:
                height += GetHeightOfEvent(property, absEventName);
                break;
            case (int)DXIntEvent.EventType.Negative:
                height += GetHeightOfEvent(property, negativeEventName);
                break;
        }
    }
}
#endif