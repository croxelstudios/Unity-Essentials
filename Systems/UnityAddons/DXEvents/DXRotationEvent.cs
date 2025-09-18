using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXRotationEvent : DXTypedEvent<Vector3>
{
    [SerializeField]
    protected EventType[] types = new EventType[] { EventType.EulerAngles };
    [SerializeField]
    UnityEvent<Quaternion> quaternionEvent = null;
    [SerializeField]
    UnityEvent<float> angleEvent = null;
    [SerializeField]
    UnityEvent<Vector3> axisEvent = null;
    [SerializeField]
    UnityEvent<float> xEvent = null;
    [SerializeField]
    UnityEvent<float> yEvent = null;
    [SerializeField]
    UnityEvent<float> zEvent = null;

    public DXRotationEvent() { types = new EventType[] { EventType.EulerAngles }; }

    public enum EventType
    { EulerAngles, Quaternion, Angle, Axis, EulerX, EulerY, EulerZ }

    public void Invoke(Quaternion arg0)
    {
        bool hasQuaternion = false;
        bool hasEulerAngles = false;
        bool hasAngle = false;
        bool hasAxis = false;
        bool hasX = false;
        bool hasY = false;
        bool hasZ = false;
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case EventType.EulerAngles:
                    hasEulerAngles = true;
                    break;
                case EventType.Quaternion:
                    hasQuaternion = true;
                    break;
                case EventType.Angle:
                    hasAngle = true;
                    break;
                case EventType.Axis:
                    hasAxis = true;
                    break;
                case EventType.EulerX:
                    hasX = true;
                    break;
                case EventType.EulerY:
                    hasY = true;
                    break;
                case EventType.EulerZ:
                    hasZ = true;
                    break;
            }
        }

        arg0.ToAngleAxis(out float angle, out Vector3 axis);
        if (hasEulerAngles)
            unityEvent?.Invoke(arg0.eulerAngles);
        if (hasQuaternion)
            quaternionEvent?.Invoke(arg0);
        if (hasAngle)
            angleEvent?.Invoke(angle);
        if (hasAxis)
            axisEvent?.Invoke(axis);
        if (hasX)
            xEvent?.Invoke(arg0.eulerAngles.x);
        if (hasY)
            yEvent?.Invoke(arg0.eulerAngles.y);
        if (hasZ)
            zEvent?.Invoke(arg0.eulerAngles.z);
    }

    public override void Invoke(Vector3 arg0)
    {
        Invoke(Quaternion.Euler(arg0));
    }

    public void AddListener(UnityAction<Quaternion> call)
    {
        quaternionEvent = quaternionEvent.CreateAddListener(call);
    }

    public void RemoveListener(UnityAction<Quaternion> call)
    {
        quaternionEvent.SmartRemoveListener(call);
    }

    public void AddListener_Angle(UnityAction<float> call)
    {
        angleEvent = angleEvent.CreateAddListener(call);
    }

    public void RemoveListener_Angle(UnityAction<float> call)
    {
        angleEvent.SmartRemoveListener(call);
    }

    public void AddListener_Axis(UnityAction<Vector3> call)
    {
        axisEvent = axisEvent.CreateAddListener(call);
    }

    public void RemoveListener_Axis(UnityAction<Vector3> call)
    {
        axisEvent.SmartRemoveListener(call);
    }

    public void AddListener_X(UnityAction<float> call)
    {
        xEvent = xEvent.CreateAddListener(call);
    }

    public void RemoveListener_X(UnityAction<float> call)
    {
        xEvent.SmartRemoveListener(call);
    }

    public void AddListener_Y(UnityAction<float> call)
    {
        yEvent = yEvent.CreateAddListener(call);
    }

    public void RemoveListener_Y(UnityAction<float> call)
    {
        yEvent.SmartRemoveListener(call);
    }

    public void AddListener_Z(UnityAction<float> call)
    {
        zEvent = zEvent.CreateAddListener(call);
    }

    public void RemoveListener_Z(UnityAction<float> call)
    {
        zEvent.SmartRemoveListener(call);
    }

    public override bool IsNull()
    {
        bool isNull = true;
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case EventType.EulerAngles:
                    if ((unityEvent != null) && (unityEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.Quaternion:
                    if ((quaternionEvent != null) && (quaternionEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.Angle:
                    if ((angleEvent != null) && (angleEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.Axis:
                    if ((axisEvent != null) && (axisEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.EulerX:
                    if ((xEvent != null) && (xEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.EulerY:
                    if ((yEvent != null) && (yEvent.GetPersistentEventCount() > 0))
                        isNull = false;
                    break;
                case EventType.EulerZ:
                    if ((zEvent != null) && (zEvent.GetPersistentEventCount() > 0))
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

[CustomPropertyDrawer(typeof(DXRotationEvent))]
public class DXQuaternionEventDrawer : DXDrawerBase
{
    const string quaternionEventName = "quaternionEvent";
    const string angleEventName = "angleEvent";
    const string axisEventName = "axisEvent";
    const string xEventName = "xEvent";
    const string yEventName = "yEvent";
    const string zEventName = "zEvent";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DrawComplexDXEvent(position, property, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return GetComplexDXEventHeight(property, label);
    }

    protected override void DrawEventType(ref Rect eventRect,
        SerializedProperty property, SerializedProperty eventType)
    {
        switch (eventType.enumValueIndex)
        {
            case (int)DXRotationEvent.EventType.EulerAngles:
                eventRect = DrawSubEvent(eventRect, property, unityEventName, eventType);
                break;
            case (int)DXRotationEvent.EventType.Quaternion:
                eventRect = DrawSubEvent(eventRect, property, quaternionEventName, eventType);
                break;
            case (int)DXRotationEvent.EventType.Angle:
                eventRect = DrawSubEvent(eventRect, property, angleEventName, eventType);
                break;
            case (int)DXRotationEvent.EventType.Axis:
                eventRect = DrawSubEvent(eventRect, property, axisEventName, eventType);
                break;
            case (int)DXRotationEvent.EventType.EulerX:
                eventRect = DrawSubEvent(eventRect, property, xEventName, eventType);
                break;
            case (int)DXRotationEvent.EventType.EulerY:
                eventRect = DrawSubEvent(eventRect, property, yEventName, eventType);
                break;
            case (int)DXRotationEvent.EventType.EulerZ:
                eventRect = DrawSubEvent(eventRect, property, zEventName, eventType);
                break;
        }
    }

    protected override void GetEventTypeHeight(ref float height,
        SerializedProperty property, SerializedProperty eventType)
    {
        switch (eventType.enumValueIndex)
        {
            case (int)DXRotationEvent.EventType.EulerAngles:
                height += GetHeightOfEvent(property, unityEventName);
                break;
            case (int)DXRotationEvent.EventType.Quaternion:
                height += GetHeightOfEvent(property, quaternionEventName);
                break;
            case (int)DXRotationEvent.EventType.Angle:
                height += GetHeightOfEvent(property, angleEventName);
                break;
            case (int)DXRotationEvent.EventType.Axis:
                height += GetHeightOfEvent(property, axisEventName);
                break;
            case (int)DXRotationEvent.EventType.EulerX:
                height += GetHeightOfEvent(property, xEventName);
                break;
            case (int)DXRotationEvent.EventType.EulerY:
                height += GetHeightOfEvent(property, yEventName);
                break;
            case (int)DXRotationEvent.EventType.EulerZ:
                height += GetHeightOfEvent(property, zEventName);
                break;
        }
    }
}
#endif
