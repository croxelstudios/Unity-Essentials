using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXVectorEvent
{
    [SerializeField]
    protected EventType[] types = new EventType[] { EventType.Vector3 };
    [SerializeField]
    Vector3Event unityEvent = null;
    [SerializeField]
    Vector2Event vector2Event = null;
    [SerializeField]
    FloatEvent magnitude = null;
    [SerializeField]
    Vector3Event unityEventNormal = null;
    [SerializeField]
    Vector2Event vector2EventNormal = null;
    [SerializeField]
    FloatEvent xEvent = null;
    [SerializeField]
    FloatEvent yEvent = null;
    [SerializeField]
    FloatEvent zEvent = null;
    [SerializeField]
    FloatEvent magnitudeNonZero = null;
    [SerializeField]
    UnityEvent magnitudeZero = null;

    public DXVectorEvent() { types = new EventType[] { EventType.Vector3 }; }

    [Serializable]
    public class Vector3Event : UnityEvent<Vector3> { }
    [Serializable]
    public class Vector2Event : UnityEvent<Vector2> { }
    [Serializable]
    public class FloatEvent : UnityEvent<float> { }

    public enum EventType
    {
        Vector3, Vector2, Magnitude,
        NormalizedVector3, NormalizedVector2, X, Y, Z, 
        MagnitudeNonZero, MagnitudeZero
    }

    public void Invoke(Vector3 arg0)
    {
        bool hasVector3 = false;
        bool hasVector2 = false;
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

    public void Invoke(Vector2 arg0)
    {
        Invoke((Vector3)arg0);
    }

    public void AddListener(UnityAction<Vector3> call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction<Vector3> call)
    {
        unityEvent.RemoveListener(call);
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
        magnitude.RemoveListener(call);
    }

    public void AddListenerNormal(UnityAction<Vector3> call)
    {
        unityEventNormal.AddListener(call);
    }

    public void RemoveListenerNormal(UnityAction<Vector3> call)
    {
        unityEventNormal.RemoveListener(call);
    }

    public void AddListenerNormal(UnityAction<Vector2> call)
    {
        vector2EventNormal.AddListener(call);
    }

    public void RemoveListenerNormal(UnityAction<Vector2> call)
    {
        vector2EventNormal.RemoveListener(call);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXVectorEvent))]
public class DXVectorEventDrawer : DXDrawerBase
{
    const string eventTypesName = "types";
    const string vector2EventName = "vector2Event";
    const string magnitudeEventName = "magnitude";
    const string vector3NormalEventName = "unityEventNormal";
    const string vector2NormalEventName = "vector2EventNormal";
    const string xEventName = "xEvent";
    const string yEventName = "yEvent";
    const string zEventName = "zEvent";
    const string magnitudeNonZeroEventName = "magnitudeNonZero";
    const string magnitudeZeroEventName = "magnitudeZero";
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
                case (int)DXVectorEvent.EventType.Vector3:
                    eventRect = DrawSubEvent(eventRect, property, unityEventName, "     ");
                    break;
                case (int)DXVectorEvent.EventType.Vector2:
                    eventRect = DrawSubEvent(eventRect, property, vector2EventName, "     ");
                    break;
                case (int)DXVectorEvent.EventType.Magnitude:
                    eventRect = DrawSubEvent(eventRect, property, magnitudeEventName, "             ");
                    break;
                case (int)DXVectorEvent.EventType.NormalizedVector3:
                    eventRect = DrawSubEvent(eventRect, property, vector3NormalEventName, "                            ");
                    break;
                case (int)DXVectorEvent.EventType.NormalizedVector2:
                    eventRect = DrawSubEvent(eventRect, property, vector2NormalEventName, "                            ");
                    break;
                case (int)DXVectorEvent.EventType.X:
                    eventRect = DrawSubEvent(eventRect, property, xEventName, "");
                    break;
                case (int)DXVectorEvent.EventType.Y:
                    eventRect = DrawSubEvent(eventRect, property, yEventName, "");
                    break;
                case (int)DXVectorEvent.EventType.Z:
                    eventRect = DrawSubEvent(eventRect, property, zEventName, "");
                    break;
                case (int)DXVectorEvent.EventType.MagnitudeNonZero:
                    eventRect = DrawSubEvent(eventRect, property, magnitudeNonZeroEventName, "                               ");
                    break;
                case (int)DXVectorEvent.EventType.MagnitudeZero:
                    eventRect = DrawSubEvent(eventRect, property, magnitudeZeroEventName, "                                  ");
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
                case (int)DXVectorEvent.EventType.Vector3:
                    height += GetHeightOfEvent(property, unityEventName);
                    break;
                case (int)DXVectorEvent.EventType.Vector2:
                    height += GetHeightOfEvent(property, vector2EventName);
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