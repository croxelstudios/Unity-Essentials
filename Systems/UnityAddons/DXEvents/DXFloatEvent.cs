using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXFloatEvent : DXTypedEvent<float>
{
    [SerializeField]
    protected EventType[] types = new EventType[] { EventType.Float };
    [SerializeField]
    UnityEvent<float> absEvent = null;
    [SerializeField]
    UnityEvent<float> negativeEvent = null;
    [SerializeField]
    UnityEvent<float> oneMinusEvent = null;

    public DXFloatEvent() { types = new EventType[] { EventType.Float }; }

    public enum EventType
    {
        Float, Abs, Negative, OneMinus
    }

    public override void Invoke(float arg0)
    {
        bool hasFloat = false;
        bool hasAbs = false;
        bool hasNegative = false;
        bool hasOneMinus = false;
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case EventType.Float:
                    hasFloat = true;
                    break;
                case EventType.Abs:
                    hasAbs = true;
                    break;
                case EventType.Negative:
                    hasNegative = true;
                    break;
                case EventType.OneMinus:
                    hasOneMinus = true;
                    break;
            }
        }
        if (hasFloat)
            unityEvent?.Invoke(arg0);
        if (hasAbs)
            absEvent?.Invoke(Mathf.Abs(arg0));
        if (hasNegative)
            negativeEvent?.Invoke(-arg0);
        if (hasOneMinus)
            oneMinusEvent?.Invoke(1f - arg0);
    }

    public void AddListener_Abs(UnityAction<float> call)
    {
        absEvent.AddListener(call);
    }

    public void RemoveListener_Abs(UnityAction<float> call)
    {
        absEvent.RemoveListener(call);
    }

    public void AddListener_Negative(UnityAction<float> call)
    {
        negativeEvent.AddListener(call);
    }

    public void RemoveListener_Negative(UnityAction<float> call)
    {
        negativeEvent.RemoveListener(call);
    }

    public void AddListener_OneMinus(UnityAction<float> call)
    {
        oneMinusEvent.AddListener(call);
    }

    public void RemoveListener_OneMinus(UnityAction<float> call)
    {
        oneMinusEvent.RemoveListener(call);
    }

    public override bool IsNull()
    {
        bool isNull = true;
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case EventType.Float:
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
                case EventType.OneMinus:
                    if ((oneMinusEvent != null) || (oneMinusEvent.GetPersistentEventCount() > 0))
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

[CustomPropertyDrawer(typeof(DXFloatEvent))]
public class DXFloatEventDrawer : DXDrawerBase
{
    const string absEventName = "absEvent";
    const string negativeEventName = "negativeEvent";
    const string oneMinusEventName = "oneMinusEvent";

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
            case (int)DXFloatEvent.EventType.Float:
                eventRect = DrawSubEvent(eventRect, property, unityEventName, eventType);
                break;
            case (int)DXFloatEvent.EventType.Abs:
                eventRect = DrawSubEvent(eventRect, property, absEventName, eventType);
                break;
            case (int)DXFloatEvent.EventType.Negative:
                eventRect = DrawSubEvent(eventRect, property, negativeEventName, eventType);
                break;
            case (int)DXFloatEvent.EventType.OneMinus:
                eventRect = DrawSubEvent(eventRect, property, oneMinusEventName, eventType);
                break;
        }
    }

    protected override void GetEventTypeHeight(ref float height,
        SerializedProperty property, SerializedProperty eventType)
    {
        switch (eventType.enumValueIndex)
        {
            case (int)DXFloatEvent.EventType.Float:
                height += GetHeightOfEvent(property, unityEventName);
                break;
            case (int)DXFloatEvent.EventType.Abs:
                height += GetHeightOfEvent(property, absEventName);
                break;
            case (int)DXFloatEvent.EventType.Negative:
                height += GetHeightOfEvent(property, negativeEventName);
                break;
            case (int)DXFloatEvent.EventType.OneMinus:
                height += GetHeightOfEvent(property, oneMinusEventName);
                break;
        }
    }
}
#endif