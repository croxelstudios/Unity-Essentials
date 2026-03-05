using System;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
#endif

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NamedListAttribute : BasePropertyRefAttribute
{
    public NamedListAttribute(string propArrayName) : base(propArrayName)
    {
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(NamedListAttribute))]
public class NamedListAttribute_Drawer : PropertyDrawer
{
    static Dictionary<SerializedPropertyData, ReorderableOperation> reorderableLists;
    static Dictionary<SerializedPropertyData, int> lastFrame;
    static List<SerializedPropertyData> toRemove;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        property.Next(false);

        if ((property == null) || !property.isArray)
            return 0;

        if (TryGetReorderableOperation(property, out ReorderableOperation operation))
            return operation.GetHeight();
        else return 0f;

        #region Each element version
        // NOTE: This is the way of doing it for each element,
        // but the problem is this doesn't update the names array

        //if (!GetData(property, out SerializedProperty name, out int index))
        //    return EditorGUI.GetPropertyHeight(property);

        //float propertyHeight;
        //if (property.isExpanded)
        //{
        //    propertyHeight = EditorGUI.GetPropertyHeight(name);

        //    if ((property.type == typeof(UnityEvent).Name) ||
        //        (property.type == typeof(DXEvent).Name)) //TO DO: Very crappy
        //        propertyHeight += EditorGUI.GetPropertyHeight(property, true);
        //    else
        //    {
        //        int count = 0;
        //        foreach (SerializedProperty child in property.GetChildren())
        //        {
        //            propertyHeight += EditorGUI.GetPropertyHeight(child, true);
        //            propertyHeight += 2;
        //            count++;
        //        }
        //        if (count <= 0) propertyHeight += EditorGUI.GetPropertyHeight(property, true);
        //    }
        //}
        //else propertyHeight = EditorGUIUtility.singleLineHeight;

        //float spacing = EditorGUIUtility.singleLineHeight / 4;

        //return propertyHeight + spacing;
        #endregion
    }

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        property.Next(false);

        if ((property == null) || !property.isArray)
            return;

        if (TryGetReorderableOperation(property, out ReorderableOperation operation))
            operation.DoList(rect);

        property.serializedObject.ApplyModifiedProperties();

        #region Each element version
        // NOTE: This is the way of doing it for each element,
        // but the problem is this doesn't update the names array

        //if (!GetData(property, out SerializedProperty name, out int index))
        //    return;

        //rect.y += 2;
        //string n = name?.stringValue;
        //string elementNumber = index + ". ";
        //string elementTitle = n.IsNullOrEmpty() ? "Unnamed" : n;

        //float widthSum = -10f;
        //float widthMult = 1f;

        //property.isExpanded = EditorGUI.Foldout(new Rect(rect.x, rect.y,
        //    (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
        //    property.isExpanded, property.isExpanded ? "" : elementNumber);

        //if (property.isExpanded)
        //{
        //    rect.x += 10;
        //    name.PropertyField(rect, widthSum, widthMult);

        //    float y = rect.y + EditorGUI.GetPropertyHeight(name) + 2;
        //    if ((property.type == typeof(UnityEvent).Name) ||
        //        (property.type == typeof(DXEvent).Name)) //TO DO: Very crappy
        //        property.PropertyField(elementTitle, y, rect, widthSum, widthMult);
        //    else
        //    {
        //        int count = 0;
        //        foreach (SerializedProperty child in property.GetChildren())
        //        {
        //            child.PropertyField(y, rect, widthSum, widthMult);
        //            y += EditorGUI.GetPropertyHeight(child, new GUIContent(child.displayName), true);
        //            y += 2;
        //            count++;
        //        }
        //        if (count <= 0)
        //            property.PropertyField(elementTitle, y, rect, widthSum, widthMult);
        //    }
        //}
        //else
        //{
        //    rect.x += 35;
        //    rect.y -= 2;
        //    EditorGUI.LabelField(rect, elementTitle);
        //}
        #endregion
    }

    #region Each element version
    //bool GetData(SerializedProperty property, out SerializedProperty name, out int index)
    //{
    //    name = null;
    //    index = property.GetArrayElementIndex();
    //    if (index < 0)
    //        return false;
    //    name = GetNameProp(property, index);
    //    return true;
    //}

    //SerializedProperty GetNameProp(SerializedProperty property, int index)
    //{
    //    SerializedProperty namesArray = GetNamesProp(property);
    //    if (namesArray.arraySize <= index)
    //    {
    //        namesArray.arraySize = index + 1;
    //        property.serializedObject.ApplyModifiedProperties();
    //    }
    //    return namesArray.GetArrayElementAtIndex(index);
    //}
    #endregion

    bool TryGetReorderableOperation(SerializedProperty property, out ReorderableOperation operation)
    {
        operation = GetReorderableList(property);
        try
        { operation.GetHeight(); }
        catch
        {
            reorderableLists.Clear();
            operation = null;
            return false;
        }
        return true;
    }

    SerializedProperty GetNamesProp(SerializedProperty property)
    {
        NamedListAttribute att = attribute as NamedListAttribute;
        return GetNamesProp(property, att.propPath);
    }

    static SerializedProperty GetNamesProp(SerializedProperty property, string propPath)
    {
        SerializedObject obj = property.serializedObject;
        return obj.GetSerializedProperty(propPath);
    }

    ReorderableOperation GetReorderableList(SerializedProperty property)
    {
        reorderableLists = reorderableLists.CreateIfNull();
        lastFrame = lastFrame.CreateIfNull();
        toRemove = toRemove.CreateIfNull();
        //EditorApplication.update += OnUpdate;

        foreach (ReorderableOperation op in reorderableLists.Values)
            if (!op.IsValid())
            {
                reorderableLists.Clear();
                break;
            }

        SerializedPropertyData propertyData = new SerializedPropertyData(property);
        SerializedObject serializedObject = property.serializedObject;
        SerializedProperty names = GetNamesProp(property);

        ReorderableOperation operation;
        if (!reorderableLists.TryGetValue(propertyData, out operation))
        {
            operation = new ReorderableOperation(property, names);
            reorderableLists.Add(propertyData, operation);
            serializedObject.ApplyModifiedProperties();
        }
        property.arraySize = names.arraySize;
        lastFrame.Set(propertyData, EditorTime.frameCount);
        return operation;
    }

    public class ReorderableOperation
    {
        SerializedProperty array;
        SerializedObject serializedObject;
        SerializedProperty names;
        ReorderableList list;
        int lastIndex;
        public event NameArrayChangeDelegate onNameArrayChange;
        public delegate void NameArrayChangeDelegate(bool priorizeLocal = true);

        public ReorderableOperation(SerializedProperty property, SerializedProperty names)
        {
            array = property;
            serializedObject = property.serializedObject;
            this.names = names;
            list = null;
            lastIndex = 0;
            onNameArrayChange = null;
            list = new ReorderableList(serializedObject, array, true, true, true, true)
            {
                elementHeightCallback = ElementHeightCallback,
                drawHeaderCallback = DrawListHeader,
                drawElementCallback = DrawElement,
                onSelectCallback = OnSelectElement,
                onReorderCallback = OnReorderList,
                onAddCallback = OnAddElement,
                onRemoveCallback = OnRemoveElement
            };
        }

        void NameArrayChanged(bool priorizeLocal = true)
        {
            onNameArrayChange?.Invoke(priorizeLocal);
        }

        public float GetHeight()
        {
            return list.GetHeight();
        }

        public bool IsValid()
        {
            try
            { return (serializedObject != null) && (serializedObject.targetObject != null); }
            catch
            { return false; }
        }

        #region ReorderableList callbacks
        void DrawListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, array.displayName);
        }

        private float ElementHeightCallback(int index)
        {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            float propertyHeight;
            if (element.isExpanded)
            {
                propertyHeight = EditorGUI.GetPropertyHeight(names.GetArrayElementAtIndex(index));

                if ((element.type == typeof(UnityEvent).Name) ||
                    (element.type == typeof(DXEvent).Name)) //TO DO: Very crappy
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
            SerializedProperty elementName = names.GetArrayElementAtIndex(index);
            rect.y += 2;

            //We get the name property of our element so we can display this in our list
            string n = elementName?.stringValue;
            string elementNumber = index + ". ";
            string elementTitle = n.IsNullOrEmpty() ? "Unnamed" : n;

            float widthSum = -10f;
            float widthMult = 1f;

            element.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y,
                (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                element.isExpanded, element.isExpanded ? "" : elementNumber);

            if (element.isExpanded)
            {
                //Draw name
                elementName.PropertyField(rect, widthSum, widthMult);

                //Draw event
                float y = rect.y + EditorGUI.GetPropertyHeight(elementName) + 2;
                if ((element.type == typeof(UnityEvent).Name) ||
                    (element.type == typeof(DXEvent).Name)) //TO DO: Very crappy
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
                rect.x += 35f;
                rect.y -= 3f;
                EditorGUI.LabelField(rect, elementTitle);
            }
        }

        public void DoLayoutList()
        {
            list.DoLayoutList();
        }

        public void DoList(Rect rect)
        {
            list.DoList(rect);
        }

        void OnSelectElement(ReorderableList list)
        {
            lastIndex = list.index;
        }

        void OnReorderList(ReorderableList list)
        {
            #region Reorder names
            string selectedString = names.GetArrayElementAtIndex(lastIndex).stringValue;
            if (list.index > lastIndex)
            {
                for (int i = lastIndex + 1; i <= list.index; i++)
                {
                    names.GetArrayElementAtIndex(i - 1).stringValue =
                        names.GetArrayElementAtIndex(i).stringValue;
                }
            }
            else if (list.index < lastIndex)
            {
                for (int i = lastIndex - 1; i >= list.index; i--)
                {
                    names.GetArrayElementAtIndex(i + 1).stringValue =
                        names.GetArrayElementAtIndex(i).stringValue;
                }
            }
            names.GetArrayElementAtIndex(list.index).stringValue = selectedString;
            #endregion

            lastIndex = list.index;

            serializedObject.ApplyModifiedProperties();
            NameArrayChanged(false);
        }

        void OnAddElement(ReorderableList list)
        {
            names.arraySize++;
            new ReorderableList.Defaults().DoAddButton(list);

            lastIndex = list.index;

            serializedObject.ApplyModifiedProperties();
            NameArrayChanged(false);
        }

        void OnRemoveElement(ReorderableList list)
        {
            names.DeleteArrayElementAtIndex(lastIndex);

            new ReorderableList.Defaults().DoRemoveButton(list);

            lastIndex = list.index;

            serializedObject.ApplyModifiedProperties();

            NameArrayChanged(false);
        }
        #endregion
    }

    //void OnUpdate()
    //{
    //    foreach (KeyValuePair<SerializedPropertyData, int> last in lastFrame)
    //        if (EditorTime.frameCount - last.Value > 2)
    //            toRemove.Add(last.Key);
    //    foreach (SerializedPropertyData data in toRemove)
    //    {
    //        reorderableLists.Remove(data);
    //        lastFrame.Remove(data);
    //    }
    //    toRemove.Clear();
    //}
}
#endif
