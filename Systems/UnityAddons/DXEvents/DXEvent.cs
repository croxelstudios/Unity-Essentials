using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXEvent
{
    [SerializeField]
    UnityEvent unityEvent = null;
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

    public bool IsNull()
    {
        return unityEvent == null || unityEvent.GetPersistentEventCount() <= 0;
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
    protected const string unityEventName = "unityEvent";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty unityEventProperty =
            property.FindPropertyRelative(unityEventName);
        Rect eventRect = position;
        EditorGUI.PropertyField(eventRect, unityEventProperty,
            new GUIContent(property.displayName));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty unityEventProperty = property.FindPropertyRelative(unityEventName);
        return EditorGUI.GetPropertyHeight(unityEventProperty);
    }
}
#endif