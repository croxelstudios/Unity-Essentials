using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXRotationEvent
{
    [SerializeField]
    protected EventType[] types = new EventType[] { EventType.EulerAngles };
    [SerializeField]
    Vector3Event unityEvent = null;
    [SerializeField]
    QuaternionEvent quaternionEvent = null;
    [SerializeField]
    FloatEvent angleEvent = null;
    [SerializeField]
    Vector3Event axisEvent = null;
    [SerializeField]
    FloatEvent xEvent = null;
    [SerializeField]
    FloatEvent yEvent = null;
    [SerializeField]
    FloatEvent zEvent = null;

    public DXRotationEvent() { types = new EventType[] { EventType.EulerAngles }; }

    [Serializable]
    public class QuaternionEvent : UnityEvent<Quaternion> { }
    [Serializable]
    public class Vector3Event : UnityEvent<Vector3> { }
    [Serializable]
    public class FloatEvent : UnityEvent<float> { }

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

    public void Invoke(Vector3 arg0)
    {
        Invoke(Quaternion.Euler(arg0));
    }

    public void AddListener(UnityAction<Quaternion> call)
    {
        quaternionEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<Quaternion> call)
    {
        quaternionEvent.RemoveListener(call);
    }

    public void AddListener(UnityAction<Vector3> call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<Vector3> call)
    {
        unityEvent.RemoveListener(call);
    }

    public void AddListenerAngle(UnityAction<float> call)
    {
        angleEvent.AddListener(call);
    }

    public void RemoveListenerAngle(UnityAction<float> call)
    {
        angleEvent.RemoveListener(call);
    }

    public void AddListenerAxis(UnityAction<Vector3> call)
    {
        axisEvent.AddListener(call);
    }

    public void RemoveListenerAxis(UnityAction<Vector3> call)
    {
        axisEvent.RemoveListener(call);
    }

    public void AddListenerX(UnityAction<float> call)
    {
        xEvent.AddListener(call);
    }

    public void RemoveListenerX(UnityAction<float> call)
    {
        xEvent.RemoveListener(call);
    }

    public void AddListenerY(UnityAction<float> call)
    {
        yEvent.AddListener(call);
    }

    public void RemoveListenerY(UnityAction<float> call)
    {
        yEvent.RemoveListener(call);
    }

    public void AddListenerZ(UnityAction<float> call)
    {
        zEvent.AddListener(call);
    }

    public void RemoveListenerZ(UnityAction<float> call)
    {
        zEvent.RemoveListener(call);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXRotationEvent))]
public class DXQuaternionEventDrawer : DXDrawerBase
{
    const string eventTypesName = "types";
    const string quaternionEventName = "quaternionEvent";
    const string angleEventName = "angleEvent";
    const string axisEventName = "axisEvent";
    const string xEventName = "xEvent";
    const string yEventName = "yEvent";
    const string zEventName = "zEvent";
    const float buttonSizeX = 80f;
    const float buttonSizeY = 20f;
    const float margin = 10f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Init
        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.alignment = TextAnchor.UpperLeft;
        SerializedProperty eventTypes = property.FindPropertyRelative(eventTypesName);

        //Window
        Rect windowRect =
            new Rect(position.min,
            new Vector2(position.width, position.height - margin));
        if (Event.current.type == EventType.Repaint)
            windowStyle.Draw(windowRect, new GUIContent(""), 0);

        //Label
        Rect labelRect =
            new Rect(new Vector2(position.xMin + 3f, position.yMin - 1f),
            new Vector2(position.width, EditorGUIUtility.singleLineHeight));
        EditorGUI.LabelField(labelRect, new GUIContent(property.displayName), boxStyle);
        position.y += EditorGUIUtility.singleLineHeight;

        //Events
        Rect eventRect =
            new Rect(new Vector2(position.xMin + 3f, position.yMin),
            new Vector2(position.width - 6f, position.height));
        for (int i = 0; i < eventTypes.arraySize; i++)
        {
            SerializedProperty eventType = eventTypes.GetArrayElementAtIndex(i);

            //Popup rect
            Vector2 popupSize = GUI.skin.label.CalcSize(
                new GUIContent(eventType.enumDisplayNames[eventType.enumValueIndex]));
            popupSize.x += 27f;
            Rect popupRect =
                new Rect(new Vector2(eventRect.xMin + 1f, eventRect.yMin + 1f), popupSize);

            //Event
            switch (eventType.enumValueIndex)
            {
                case (int)DXRotationEvent.EventType.EulerAngles:
                    eventRect = DrawSubEvent(eventRect, property, unityEventName, "                             ");
                    break;
                case (int)DXRotationEvent.EventType.Quaternion:
                    eventRect = DrawSubEvent(eventRect, property, quaternionEventName, "                          ");
                    break;
                case (int)DXRotationEvent.EventType.Angle:
                    eventRect = DrawSubEvent(eventRect, property, angleEventName, "                 ");
                    break;
                case (int)DXRotationEvent.EventType.Axis:
                    eventRect = DrawSubEvent(eventRect, property, axisEventName, "               ");
                    break;
                case (int)DXRotationEvent.EventType.EulerX:
                    eventRect = DrawSubEvent(eventRect, property, xEventName, "                    ");
                    break;
                case (int)DXRotationEvent.EventType.EulerY:
                    eventRect = DrawSubEvent(eventRect, property, yEventName, "                    ");
                    break;
                case (int)DXRotationEvent.EventType.EulerZ:
                    eventRect = DrawSubEvent(eventRect, property, zEventName, "                    ");
                    break;
            }

            //Popup
            EditorGUI.PropertyField(popupRect, eventType, new GUIContent(""));
        }
        position.y = eventRect.y;

        //Plus and less button
        if (eventTypes.arraySize > 0)
        {
            Rect lessButtonRect =
                new Rect(new Vector2(position.center.x - buttonSizeX,
                position.yMin - (buttonSizeY * 0.8f)),
                new Vector2(buttonSizeX, buttonSizeY));
            Rect plusButtonRect =
                new Rect(new Vector2(position.center.x,
                position.yMin - (buttonSizeY * 0.8f)),
                new Vector2(buttonSizeX, buttonSizeY));
            if (GUI.Button(plusButtonRect, "+"))
                AddEvent(eventTypes);
            if (GUI.Button(lessButtonRect, "-"))
                RemoveEvent(eventTypes);
        }
        else
        {
            Rect plusButtonRect =
                new Rect(new Vector2(position.center.x - (buttonSizeX / 2),
                position.yMin - (buttonSizeY * 0.8f)),
                new Vector2(buttonSizeX, buttonSizeY));
            if (GUI.Button(plusButtonRect, "+"))
                AddEvent(eventTypes);
        }
    }

    void AddEvent(SerializedProperty eventTypes)
    {
        eventTypes.arraySize++;
        SerializedProperty newEvent =
            eventTypes.GetArrayElementAtIndex(eventTypes.arraySize - 1);
        if (eventTypes.arraySize > 1)
            newEvent.enumValueIndex =
                (int)Mathf.Repeat(newEvent.enumValueIndex + 1, newEvent.enumNames.Length);
    }

    void RemoveEvent(SerializedProperty eventTypes)
    {
        eventTypes.arraySize--;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        SerializedProperty eventTypes = property.FindPropertyRelative(eventTypesName);
        for (int i = 0; i < eventTypes.arraySize; i++)
        {
            SerializedProperty eventType = eventTypes.GetArrayElementAtIndex(i);
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
        height += margin;
        return height;
    }

    //Enummed list system
    Rect DrawSubEvent(Rect position, SerializedProperty property,
        string eventName, string name)
    {
        SerializedProperty unityEventProperty =
            property.FindPropertyRelative(eventName);
        float height = EditorGUI.GetPropertyHeight(unityEventProperty);

        Rect eventRect =
            new Rect(position.min,
            new Vector2(position.width, height));

        EditorGUI.PropertyField(eventRect, unityEventProperty, new GUIContent(name));
        position.y += height;

        return position;
    }

    float GetHeightOfEvent(SerializedProperty parentProperty, string name)
    {
        SerializedProperty unityEventProperty = parentProperty.FindPropertyRelative(name);
        return EditorGUI.GetPropertyHeight(unityEventProperty);
    }

    //Multibox system
    Rect DrawSubEvent(Rect position, SerializedProperty property, string eventName,
        string name, GUIStyle boxStyle, GUIStyle foldoutStyle, bool defaultExpand = false)
    {
        SerializedProperty unityEventProperty =
            property.FindPropertyRelative(eventName);
        float height = EditorGUI.GetPropertyHeight(unityEventProperty);

        Rect lineRect = new Rect(position.min,
            new Vector2(position.width, EditorGUIUtility.singleLineHeight));
        Rect boxRect =
            new Rect(position.min,
            new Vector2(position.width, height));
        Rect eventRect =
            new Rect(new Vector2(boxRect.xMin + 3f, boxRect.yMin + 3f),
            new Vector2(boxRect.width - 6f, boxRect.height - 6f));

        if (unityEventProperty.isExpanded ^ defaultExpand)
        {
            if (Event.current.type == EventType.Repaint)
                boxStyle.Draw(boxRect, new GUIContent(property.displayName), 0);
            EditorGUI.PropertyField(eventRect, unityEventProperty, new GUIContent(name));
            position.y += height;
        }
        else
        {
            if (Event.current.type == EventType.Repaint)
                boxStyle.Draw(lineRect, new GUIContent(property.displayName), 0);
            Rect labelRect = lineRect;
            labelRect.x += 3f;
            labelRect.y -= 1f;
            EditorGUI.LabelField(labelRect, new GUIContent(name));
            position.y += EditorGUIUtility.singleLineHeight;
        }
        unityEventProperty.isExpanded =
            EditorGUI.Foldout(lineRect,
            unityEventProperty.isExpanded ^ defaultExpand, "", foldoutStyle)
            ^ defaultExpand;
        return position;
    }

    float GetHeightOfEvent(SerializedProperty parentProperty, string name, bool reverseExpand)
    {
        SerializedProperty unityEventProperty = parentProperty.FindPropertyRelative(name);
        if (reverseExpand ^ unityEventProperty.isExpanded)
            return EditorGUI.GetPropertyHeight(unityEventProperty);
        else return EditorGUIUtility.singleLineHeight;
    }
}
#endif
