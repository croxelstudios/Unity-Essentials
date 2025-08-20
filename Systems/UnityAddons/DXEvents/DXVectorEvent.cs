using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXVectorEvent : DXTypedEvent<Vector3>
{
    [SerializeField]
    protected EventType[] types = new EventType[] { EventType.Vector3 };
    [SerializeField]
    UnityEvent<Vector2> vector2Event = null;
    [SerializeField]
    UnityEvent<Vector2> vector2XZEvent = null;
    [SerializeField]
    UnityEvent<float> magnitude = null;
    [SerializeField]
    UnityEvent<Vector3> unityEventNormal = null;
    [SerializeField]
    UnityEvent<Vector2>  vector2EventNormal = null;
    [SerializeField]
    UnityEvent<float> xEvent = null;
    [SerializeField]
    UnityEvent<float> yEvent = null;
    [SerializeField]
    UnityEvent<float> zEvent = null;
    [SerializeField]
    UnityEvent<float> magnitudeNonZero = null;
    [SerializeField]
    UnityEvent magnitudeZero = null;

    public DXVectorEvent() { types = new EventType[] { EventType.Vector3 }; }

    public enum EventType
    {
        Vector3, Vector2, Vector2XZ, Magnitude,
        NormalizedVector3, NormalizedVector2, X, Y, Z, 
        MagnitudeNonZero, MagnitudeZero
    }

    public override void Invoke(Vector3 arg0)
    {
        bool hasVector3 = false;
        bool hasVector2 = false;
        bool hasVector2XZ = false;
        bool hasMagnitude = false;
        bool hasNormalizedVector3 = false;
        bool hasNormalizedVector2 = false;
        bool hasX = false;
        bool hasY = false;
        bool hasZ = false;
        bool hasMagnitudeNonZero = false;
        bool hasMagnitudeZero = false;
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case EventType.Vector3:
                    hasVector3 = true;
                    break;
                case EventType.Vector2:
                    hasVector2 = true;
                    break;
                case EventType.Vector2XZ:
                    hasVector2XZ = true;
                    break;
                case EventType.Magnitude:
                    hasMagnitude = true;
                    break;
                case EventType.NormalizedVector3:
                    hasNormalizedVector3 = true;
                    break;
                case EventType.NormalizedVector2:
                    hasNormalizedVector2 = true;
                    break;
                case EventType.X:
                    hasX = true;
                    break;
                case EventType.Y:
                    hasY = true;
                    break;
                case EventType.Z:
                    hasZ = true;
                    break;
                case EventType.MagnitudeNonZero:
                    hasMagnitudeNonZero = true;
                    break;
                case EventType.MagnitudeZero:
                    hasMagnitudeZero = true;
                    break;
            }
        }

        if (hasVector3)
            unityEvent?.Invoke(arg0);
        if (hasVector2)
            vector2Event?.Invoke(arg0);
        if (hasVector2XZ)
            vector2XZEvent?.Invoke(new Vector2(arg0.x, arg0.z));
        if (hasMagnitude)
            magnitude?.Invoke(arg0.magnitude);
        if (hasNormalizedVector3)
            unityEventNormal?.Invoke(arg0.normalized);
        if (hasNormalizedVector2)
            vector2EventNormal?.Invoke(arg0.normalized);
        if (hasX)
            xEvent?.Invoke(arg0.x);
        if (hasY)
            yEvent?.Invoke(arg0.y);
        if (hasZ)
            zEvent?.Invoke(arg0.z);
        if (hasMagnitudeNonZero)
        {
            if (arg0.sqrMagnitude > Mathf.Epsilon)
                magnitudeNonZero?.Invoke(arg0.magnitude);
        }
        if (hasMagnitudeZero)
        {
            if (arg0.sqrMagnitude <= Mathf.Epsilon)
                magnitudeZero?.Invoke();
        }
    }

    public void AddListener(UnityAction<Vector2> call)
    {
        vector2Event.AddListener(call);
    }

    public void RemoveListener(UnityAction<Vector2> call)
    {
        vector2Event.RemoveListener(call);
    }

    public void AddListener(UnityAction<float> call)
    {
        magnitude.AddListener(call);
    }

    public void RemoveListener(UnityAction<float> call)
    {
        magnitude.RemoveListener(call);
    }

    public void AddListener_Normal(UnityAction<Vector3> call)
    {
        unityEventNormal.AddListener(call);
    }

    public void RemoveListener_Normal(UnityAction<Vector3> call)
    {
        unityEventNormal.RemoveListener(call);
    }

    public void AddListener_Normal(UnityAction<Vector2> call)
    {
        vector2EventNormal.AddListener(call);
    }

    public void RemoveListener_Normal(UnityAction<Vector2> call)
    {
        vector2EventNormal.RemoveListener(call);
    }

    public void AddListener_X(UnityAction<float> call)
    {
        xEvent.AddListener(call);
    }

    public void RemoveListener_X(UnityAction<float> call)
    {
        xEvent.RemoveListener(call);
    }

    public void AddListener_Y(UnityAction<float> call)
    {
        yEvent.AddListener(call);
    }

    public void RemoveListener_Y(UnityAction<float> call)
    {
        yEvent.RemoveListener(call);
    }

    public void AddListener_Z(UnityAction<float> call)
    {
        zEvent.AddListener(call);
    }

    public void RemoveListener_Z(UnityAction<float> call)
    {
        zEvent.RemoveListener(call);
    }

    public void AddListener_MagnitudeNonZero(UnityAction<float> call)
    {
        magnitudeNonZero.AddListener(call);
    }

    public void RemoveListener_MagnitudeNonZero(UnityAction<float> call)
    {
        magnitudeNonZero.RemoveListener(call);
    }

    public void AddListener_MagnitudeZero(UnityAction call)
    {
        magnitudeZero.AddListener(call);
    }

    public void RemoveListener_MagnitudeZero(UnityAction call)
    {
        magnitudeZero.RemoveListener(call);
    }

    public override bool IsNull()
    {
        bool isNull = true;
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case EventType.Vector3:
                    if ((unityEvent != null) && (unityEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.Vector2:
                    if ((vector2Event != null) && (vector2Event.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.Vector2XZ:
                    if ((vector2XZEvent != null) && (vector2XZEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.Magnitude:
                    if ((magnitude != null) && (magnitude.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.NormalizedVector3:
                    if ((unityEventNormal != null) && (unityEventNormal.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.NormalizedVector2:
                    if ((vector2EventNormal != null) && (vector2EventNormal.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.X:
                    if ((xEvent != null) && (xEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.Y:
                    if ((yEvent != null) && (yEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.Z:
                    if ((zEvent != null) && (zEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.MagnitudeNonZero:
                    if ((magnitudeNonZero != null) && (magnitudeNonZero.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.MagnitudeZero:
                    if ((magnitudeZero != null) && (magnitudeZero.GetPersistentEventCount() > 0))
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

[CustomPropertyDrawer(typeof(DXVectorEvent))]
public class DXVectorEventDrawer : DXDrawerBase
{
    const string vector2EventName = "vector2Event";
    const string vector2XZEventName = "vector2XZEvent";
    const string magnitudeEventName = "magnitude";
    const string vector3NormalEventName = "unityEventNormal";
    const string vector2NormalEventName = "vector2EventNormal";
    const string xEventName = "xEvent";
    const string yEventName = "yEvent";
    const string zEventName = "zEvent";
    const string magnitudeNonZeroEventName = "magnitudeNonZero";
    const string magnitudeZeroEventName = "magnitudeZero";

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
                eventRect = DrawSubEvent(eventRect, property, vector2EventName, eventType);
                break;
            case (int)DXVectorEvent.EventType.Vector2XZ:
                eventRect = DrawSubEvent(eventRect, property, vector2XZEventName, eventType);
                break;
            case (int)DXVectorEvent.EventType.Magnitude:
                eventRect = DrawSubEvent(eventRect, property, magnitudeEventName, eventType);
                break;
            case (int)DXVectorEvent.EventType.NormalizedVector3:
                eventRect = DrawSubEvent(eventRect, property, vector3NormalEventName, eventType);
                break;
            case (int)DXVectorEvent.EventType.NormalizedVector2:
                eventRect = DrawSubEvent(eventRect, property, vector2NormalEventName, eventType);
                break;
            case (int)DXVectorEvent.EventType.X:
                eventRect = DrawSubEvent(eventRect, property, xEventName, eventType);
                break;
            case (int)DXVectorEvent.EventType.Y:
                eventRect = DrawSubEvent(eventRect, property, yEventName, eventType);
                break;
            case (int)DXVectorEvent.EventType.Z:
                eventRect = DrawSubEvent(eventRect, property, zEventName, eventType);
                break;
            case (int)DXVectorEvent.EventType.MagnitudeNonZero:
                eventRect = DrawSubEvent(eventRect, property, magnitudeNonZeroEventName, eventType);
                break;
            case (int)DXVectorEvent.EventType.MagnitudeZero:
                eventRect = DrawSubEvent(eventRect, property, magnitudeZeroEventName, eventType);
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
                height += GetHeightOfEvent(property, vector2EventName);
                break;
            case (int)DXVectorEvent.EventType.Vector2XZ:
                height += GetHeightOfEvent(property, vector2XZEventName);
                break;
            case (int)DXVectorEvent.EventType.Magnitude:
                height += GetHeightOfEvent(property, magnitudeEventName);
                break;
            case (int)DXVectorEvent.EventType.NormalizedVector3:
                height += GetHeightOfEvent(property, vector3NormalEventName);
                break;
            case (int)DXVectorEvent.EventType.NormalizedVector2:
                height += GetHeightOfEvent(property, vector2NormalEventName);
                break;
            case (int)DXVectorEvent.EventType.X:
                height += GetHeightOfEvent(property, xEventName);
                break;
            case (int)DXVectorEvent.EventType.Y:
                height += GetHeightOfEvent(property, yEventName);
                break;
            case (int)DXVectorEvent.EventType.Z:
                height += GetHeightOfEvent(property, zEventName);
                break;
            case (int)DXVectorEvent.EventType.MagnitudeNonZero:
                height += GetHeightOfEvent(property, magnitudeNonZeroEventName);
                break;
            case (int)DXVectorEvent.EventType.MagnitudeZero:
                height += GetHeightOfEvent(property, magnitudeZeroEventName);
                break;
        }
    }
}
#endif