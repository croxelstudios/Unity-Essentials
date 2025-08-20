using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXEvent : BDXEvent<UnityEvent>
{
#if PLAYMAKER
    [SerializeField]
    string playmaker = "";

    [SerializeField]
    PlayMakerFSM[] FSMs = null;

#endif

    [SerializeField]
    UnityEvent[] overrideEvents = null;

    public void Invoke()
    {
        unityEvent?.Invoke();

        if (overrideEvents != null)
            foreach (UnityEvent extraEvent in overrideEvents)
                extraEvent?.Invoke();

#if PLAYMAKER
        if (!string.IsNullOrEmpty(playmaker))
            foreach (PlayMakerFSM fsm in FSMs)
                if (fsm != null) fsm.SendEvent(playmaker);
#endif
    }

    public void AddListener(UnityAction call)
    {
        unityEvent.AddListener(call);
    }

    public void RemoveListener(UnityAction call)
    {
        unityEvent.RemoveListener(call);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXEvent))]
public class DXEventDrawer : DXDrawerBase
{
#if PLAYMAKER
    const string playmakerStringName = "playmaker";
    const string fsmNames = "FSMs";
#endif
    const float buttonSizeX = 15f;
    const float buttonSizeY = 15f;
    const float margin = 0f;
    const string unityOverrideEventsName = "overrideEvents";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty unityEventProperty = property.FindPropertyRelative(unityEventName);
        SerializedProperty overrideEventsArray = property.FindPropertyRelative(unityOverrideEventsName);

        if (overrideEventsArray.arraySize > 0)
        {
            Rect windowRect =
                new Rect(position.min,
                new Vector2(position.width, position.height - margin));
            if (Event.current.type == EventType.Repaint)
                new GUIStyle(GUI.skin.window).Draw(windowRect, new GUIContent(""), 0);
        }
        Rect eventRect =
            new Rect(new Vector2(position.xMin + 3f, position.yMin),
            new Vector2(position.width - 6f, position.height));
        base.OnGUI(eventRect, property, label);

        int index = 0;
        overrideEventsArray.Next(true);
        eventRect.y += EditorGUI.GetPropertyHeight(unityEventProperty);
        foreach (SerializedProperty child in overrideEventsArray.GetChildren())
        {
            if (index >= 1)
            {
                float height = EditorGUI.GetPropertyHeight(child);

                Rect childRect = new Rect(eventRect.min, new Vector2(eventRect.width, height));

                EditorGUI.PropertyField(childRect, child, new GUIContent(property.displayName + " Override " + index));
                eventRect.y += height;
            }
            index++;
        }
        Rect buttonsRect = eventRect;
        buttonsRect.y -= EditorGUIUtility.singleLineHeight * 0.5f;

        Rect lessButtonRect =
               new Rect(new Vector2(eventRect.xMin,
               buttonsRect.yMin - (buttonSizeY * 0.8f)),
               new Vector2(buttonSizeX, buttonSizeY));
        Rect plusButtonRect =
            new Rect(new Vector2(eventRect.xMin + buttonSizeX,
            buttonsRect.yMin - (buttonSizeY * 0.8f)),
            new Vector2(buttonSizeX, buttonSizeY));
        if (GUI.Button(plusButtonRect, "+"))
            overrideEventsArray.arraySize++;
        if (GUI.Button(lessButtonRect, "-"))
            overrideEventsArray.arraySize--;


#if PLAYMAKER
        if (!Application.isPlaying)
        {
            SerializedProperty fsmProperty = property.FindPropertyRelative(fsmNames);
            Component component = property.serializedObject.targetObject as Component;
            if (component != null)
            {
                #region Try setting up FSMs
                PlayMakerFSM[] fsms = component.GetComponentsInParent<PlayMakerFSM>();
                int size = fsmProperty.arraySize;
                if (size > fsms.Length)
                    for (int i = size - 1; i >= fsms.Length; i--)
                        fsmProperty.DeleteArrayElementAtIndex(i);
                else if (size < fsms.Length)
                    for (int i = size; i < fsms.Length; i++)
                        fsmProperty.InsertArrayElementAtIndex(i);
                size = fsmProperty.arraySize;
                for (int i = 0; i < size; i++)
                    fsmProperty.GetArrayElementAtIndex(i).objectReferenceValue = fsms[i];
                #endregion

                if (size > 0)
                {
                    SerializedProperty playmakerStringProperty =
                        property.FindPropertyRelative(playmakerStringName);

                    //if (playmakerStringProperty.stringValue == "")
                    //    playmakerStringProperty.stringValue =
                    //        component.GetType().ToString() + "_" + property.name;

                    Rect stringRect = position;
                    //stringRect.width = 500f;
                    stringRect.height = EditorGUIUtility.singleLineHeight;
                    float widthVariation = 100f;
                    stringRect.width -= widthVariation + 1f;
                    stringRect.x += widthVariation;
                    stringRect.y += 1f;

                    TextAnchor orginalAlignment = EditorStyles.label.alignment;
                    int orginalPadding = EditorStyles.label.padding.right;
                    EditorStyles.label.alignment = TextAnchor.MiddleRight;
                    EditorStyles.label.padding.right += 2;

                    EditorGUI.PropertyField(stringRect, playmakerStringProperty,
                        new GUIContent("Playmaker"));

                    EditorStyles.label.alignment = orginalAlignment;
                    EditorStyles.label.padding.right = orginalPadding;
                }
            }
        }
#endif
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = base.GetPropertyHeight(property, label);
        SerializedProperty overrideEventsArray = property.FindPropertyRelative(unityOverrideEventsName);

        overrideEventsArray.Next(true);
        int index = 0;
        foreach (SerializedProperty child in overrideEventsArray.GetChildren())
        {
            if (index >= 1)
                height += EditorGUI.GetPropertyHeight(child);
            index++;
        }

        return height;
    }
}

public class DXDrawerBase : PropertyDrawer
{
    const string eventTypesName = "types";
    protected const string unityEventName = "unityEvent";
    const float buttonSizeX = 80f;
    const float buttonSizeY = 20f;
    const float margin = 10f;

    //Simple event
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty unityEventProperty =
            property.FindPropertyRelative(unityEventName);
        Rect eventRect = position;
        CustomEventDrawer(eventRect, unityEventProperty,
            new GUIContent(property.displayName));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty unityEventProperty = property.FindPropertyRelative(unityEventName);
        return CustomEventHeight(unityEventProperty);
    }

    //Call custom drawer
    void CustomEventDrawer(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label);
    }

    //Call custom drawer
    float CustomEventHeight(SerializedProperty property)
    {
        return EditorGUI.GetPropertyHeight(property);
    }

    //ComplexDXEvent system
    protected void DrawComplexDXEvent(Rect position, SerializedProperty property, GUIContent label)
    {
        //Init
        SerializedProperty eventTypes = property.FindPropertyRelative(eventTypesName);

        //Window
        DrawWindow(position);

        //Label
        DrawLabel(ref position, property);

        //Events
        Rect eventRect = EventRect(position);
        for (int i = 0; i < eventTypes.arraySize; i++)
        {
            SerializedProperty eventType = eventTypes.GetArrayElementAtIndex(i);

            //Popup rect
            Rect popupRect = PopupRect(eventRect, eventType);

            //Event
            DrawEventType(ref eventRect, property, eventType);

            //Popup
            EditorGUI.PropertyField(popupRect, eventType, new GUIContent(""));
        }
        position.y = eventRect.y;

        //Plus and less button
        DrawAddAndRemoveButtons(position, eventTypes);
    }

    protected virtual void DrawEventType(ref Rect eventRect,
        SerializedProperty property, SerializedProperty eventType)
    {
        switch (eventType.enumValueIndex)
        {
            default:
                eventRect = DrawSubEvent(eventRect, property, unityEventName, eventType);
                break;
        }
    }

    protected float GetComplexDXEventHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        SerializedProperty eventTypes = property.FindPropertyRelative(eventTypesName);
        for (int i = 0; i < eventTypes.arraySize; i++)
        {
            SerializedProperty eventType = eventTypes.GetArrayElementAtIndex(i);
            GetEventTypeHeight(ref height, property, eventType);
        }
        height += margin;
        return height;
    }

    protected virtual void GetEventTypeHeight(ref float height,
        SerializedProperty property, SerializedProperty eventType)
    {
        switch (eventType.enumValueIndex)
        {
            default:
                height += GetHeightOfEvent(property, unityEventName);
                break;
        }
    }

    void DrawWindow(Rect position)
    {
        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        Rect windowRect =
            new Rect(position.min,
            new Vector2(position.width, position.height - margin));
        if (Event.current.type == EventType.Repaint)
            windowStyle.Draw(windowRect, new GUIContent(""), 0);
    }

    void DrawLabel(ref Rect position, SerializedProperty property)
    {
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.alignment = TextAnchor.UpperLeft;
        Rect labelRect =
            new Rect(new Vector2(position.xMin + 3f, position.yMin - 1f),
            new Vector2(position.width, EditorGUIUtility.singleLineHeight));
        EditorGUI.LabelField(labelRect, new GUIContent(property.displayName), boxStyle);
        position.y += EditorGUIUtility.singleLineHeight;
    }

    Rect EventRect(Rect position)
    {
        return
            new Rect(new Vector2(position.xMin + 3f, position.yMin),
            new Vector2(position.width - 6f, position.height));
    }

    Rect PopupRect(Rect eventRect, SerializedProperty eventType)
    {
        Vector2 popupSize = GUI.skin.label.CalcSize(
            new GUIContent(eventType.enumDisplayNames[eventType.enumValueIndex]));
        popupSize.x += 27f;
        return new Rect(new Vector2(eventRect.xMin + 1f, eventRect.yMin + 1f), popupSize);
    }

    void DrawAddAndRemoveButtons(Rect position, SerializedProperty eventTypes)
    {
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

    //Enummed list system
    protected Rect DrawSubEvent(Rect position, SerializedProperty property,
        string eventName, SerializedProperty eventType)
    {
        string name = eventType.enumDisplayNames[eventType.enumValueIndex] + new string(' ', 7);
        SerializedProperty unityEventProperty =
            property.FindPropertyRelative(eventName);
        float height = EditorGUI.GetPropertyHeight(unityEventProperty);

        Rect eventRect =
            new Rect(position.min,
            new Vector2(position.width, height));

        CustomEventDrawer(eventRect, unityEventProperty, new GUIContent(name));
        position.y += height;

        return position;
    }

    protected float GetHeightOfEvent(SerializedProperty parentProperty, string name)
    {
        SerializedProperty unityEventProperty = parentProperty.FindPropertyRelative(name);
        return CustomEventHeight(unityEventProperty);
    }

    //Multibox system (OBSOLETE)
    protected Rect DrawSubEvent(Rect position, SerializedProperty property, string eventName,
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

    protected float GetHeightOfEvent(SerializedProperty parentProperty, string name, bool reverseExpand)
    {
        SerializedProperty unityEventProperty = parentProperty.FindPropertyRelative(name);
        if (reverseExpand ^ unityEventProperty.isExpanded)
            return EditorGUI.GetPropertyHeight(unityEventProperty);
        else return EditorGUIUtility.singleLineHeight;
    }

    //Add and Remove buttons for complex DXEvents
    protected void AddEvent(SerializedProperty eventTypes)
    {
        eventTypes.arraySize++;
        SerializedProperty newEvent =
            eventTypes.GetArrayElementAtIndex(eventTypes.arraySize - 1);
        if (eventTypes.arraySize > 1)
            newEvent.enumValueIndex =
                (int)Mathf.Repeat(newEvent.enumValueIndex + 1, newEvent.enumNames.Length);
    }

    protected void RemoveEvent(SerializedProperty eventTypes)
    {
        eventTypes.arraySize--;
    }
}
#endif