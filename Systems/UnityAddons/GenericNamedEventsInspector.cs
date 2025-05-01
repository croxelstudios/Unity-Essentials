#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
#endif

#if UNITY_EDITOR
public class GenericNamedEvents_Inspector : Editor
{
    SerializedProperty events;
    SerializedProperty eventNames;
    ReorderableList list;
    static Dictionary<Object, List<bool>> foldouts;
    int lastIndex;

    const string eventsPropName = "events";
    const string eventNamesPropName = "eventNames";

    protected virtual void OnEnable()
    {
        eventNames = serializedObject.FindProperty(eventNamesPropName);
        events = serializedObject.FindProperty(eventsPropName);

        list = new ReorderableList(serializedObject, events, true, true, true, true)
        {
            elementHeightCallback = ElementHeightCallback,
            drawHeaderCallback = DrawListHeader,
            drawElementCallback = DrawElement,
            onSelectCallback = OnSelectElement,
            onReorderCallback = OnReorderList,
            onAddCallback = OnAddElement,
            onRemoveCallback = OnRemoveElement
        };

        Initialize();
    }

    void Initialize()
    {
        foldouts = foldouts.CreateAdd(serializedObject.targetObject,
            Enumerable.Repeat(false, events.arraySize).ToList());

        List<Object> keyList = new List<Object>(foldouts.Keys);
        for (int i = 0; i < keyList.Count; i++)
            if (keyList[i] == null) foldouts.Remove(keyList[i]);

        eventNames.arraySize = events.arraySize;

        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void NameArrayChanged(bool priorizeLocal = true)
    {

    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    #region ReorderableList callbacks
    void DrawListHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Events");
    }

    private float ElementHeightCallback(int index)
    {
        if ((foldouts == null) ||
            (!foldouts.ContainsKey(serializedObject.targetObject)) ||
            (index >= foldouts.Count))
            Initialize();
        //Gets the height of the element.

        float propertyHeight;
        if (foldouts[serializedObject.targetObject][index])
        {
            propertyHeight = EditorGUI.GetPropertyHeight(eventNames.GetArrayElementAtIndex(index));
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);

            if (element.FindPropertyRelative("m_PersistentCalls") != null) //TO DO: Very crappy
                propertyHeight += EditorGUI.GetPropertyHeight(element, true);
            else
            {
                int count = 0;
                foreach (SerializedProperty child in element.GetChildren())
                {
                    propertyHeight += EditorGUI.GetPropertyHeight(child, true);
                    count++;
                }
                if (count <= 0) propertyHeight += EditorGUI.GetPropertyHeight(element, true);
            }
        }
        else propertyHeight = EditorGUIUtility.singleLineHeight;

        float spacing = EditorGUIUtility.singleLineHeight / 4;

        return propertyHeight + spacing;
    }

    void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty elementName = eventNames.GetArrayElementAtIndex(index);
        rect.y += 2;

        //We get the name property of our element so we can display this in our list
        string elementTitle = index + ". " + (string.IsNullOrEmpty(elementName?.stringValue)
            ? "Unnamed" : elementName.stringValue);

        float widthSum = -10f;
        float widthMult = 1f;
        if (foldouts[serializedObject.targetObject][index])
        {
            //Draw foldout
            foldouts[serializedObject.targetObject][index] = EditorGUI.Foldout(
                new Rect(rect.x + 10, rect.y,
                (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                foldouts[serializedObject.targetObject][index], "");

            //Draw name
            EditorGUI.BeginChangeCheck();
            NameField(elementName, rect, widthSum, widthMult);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                NameArrayChanged();
            }

            //Draw event
            float y = rect.y + EditorGUI.GetPropertyHeight(elementName) + 2;
            if (element.FindPropertyRelative("m_PersistentCalls") != null) //TO DO: Very crappy
                element.PropertyField(elementTitle, y, rect, widthSum, widthMult);
            else
            {
                int count = 0;
                foreach (SerializedProperty child in element.GetChildren())
                {
                    child.PropertyField(y, rect, widthSum, widthMult);
                    y += EditorGUI.GetPropertyHeight(child, new GUIContent(child.displayName), true);
                    count++;
                }
                if (count <= 0)
                    element.PropertyField(elementTitle, y, rect, widthSum, widthMult);
            }
        }
        else
        {
            //Draw foldout
            foldouts[serializedObject.targetObject][index] = EditorGUI.Foldout(
                new Rect(rect.x + 10, rect.y,
                (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                foldouts[serializedObject.targetObject][index], elementTitle);
        }
    }

    protected virtual void NameField(SerializedProperty elementName, Rect rect, float widthSum = -10f, float widthMult = 1f)
    {
        elementName.PropertyField(rect, widthSum, widthMult);
    }

    protected void DoLayoutList()
    {
        list.DoLayoutList();
    }

    void OnSelectElement(ReorderableList list)
    {
        lastIndex = list.index;
    }

    void OnReorderList(ReorderableList list)
    {
        Undo.RecordObject(this, "NamedEvents Inspector Foldouts"); //TO DO: Doesn't work when you opened the foldout after moving the object
        foldouts[serializedObject.targetObject].ReorderElement(lastIndex, list.index);

        #region Reorder names
        string selectedString = eventNames.GetArrayElementAtIndex(lastIndex).stringValue;
        if (list.index > lastIndex)
        {
            for (int i = lastIndex + 1; i <= list.index; i++)
            {
                eventNames.GetArrayElementAtIndex(i - 1).stringValue =
                    eventNames.GetArrayElementAtIndex(i).stringValue;
            }
        }
        else if (list.index < lastIndex)
        {
            for (int i = lastIndex - 1; i >= list.index; i--)
            {
                eventNames.GetArrayElementAtIndex(i + 1).stringValue =
                    eventNames.GetArrayElementAtIndex(i).stringValue;
            }
        }
        eventNames.GetArrayElementAtIndex(list.index).stringValue = selectedString;
        #endregion

        lastIndex = list.index;

        serializedObject.ApplyModifiedProperties();
        NameArrayChanged(false);
    }

    void OnAddElement(ReorderableList list)
    {
        foldouts[serializedObject.targetObject].Add(false);
        eventNames.arraySize++;
        new ReorderableList.Defaults().DoAddButton(list);

        lastIndex = list.index;

        serializedObject.ApplyModifiedProperties();
        NameArrayChanged(false);
    }

    void OnRemoveElement(ReorderableList list)
    {
        foldouts[serializedObject.targetObject].RemoveAt(lastIndex);
        if (lastIndex < (list.count - 1))
        {
            for (int i = lastIndex + 1; i < list.count; i++)
            {
                eventNames.GetArrayElementAtIndex(i - 1).stringValue =
                    eventNames.GetArrayElementAtIndex(i).stringValue;
            }
        }
        eventNames.arraySize--;
        new ReorderableList.Defaults().DoRemoveButton(list);

        lastIndex = list.index;

        serializedObject.ApplyModifiedProperties();
        NameArrayChanged(false);
    }
    #endregion
}
#endif
