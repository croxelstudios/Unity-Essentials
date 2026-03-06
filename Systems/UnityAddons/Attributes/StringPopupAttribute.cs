using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using System.Linq;
using Object = UnityEngine.Object;
#endif

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class StringPopupAttribute : BasePropertyRefAttribute
{
    //TO DO: This (and everything related) is here to handle reordering of the string array.
    //It is too complicated, doesn't work well and is not being used.
    //I should find a better way to handle this or remove it.
    public string serializedPopupDataArray;

    public StringPopupAttribute(string optionsArrayName) : base(optionsArrayName)
    {
        serializedPopupDataArray = "";
    }

    public StringPopupAttribute(string optionsArrayName, string serializedPopupDataArray) : base(optionsArrayName)
    {
        this.serializedPopupDataArray = serializedPopupDataArray;
    }

#if UNITY_EDITOR
    public static bool InterpretInEventsDrawer(MethodInfo method, SerializedProperty argument, Rect argRect, SerializedProperty listenerTarget)
    {
        object[] stringPopupAttributes = null;
        if (method != null) stringPopupAttributes = method.GetCustomAttributes(typeof(StringPopupAttribute), true);
        if ((!stringPopupAttributes.IsNullOrEmpty()) &&
            ((argument.propertyType == SerializedPropertyType.Integer) ||
            (argument.propertyType == SerializedPropertyType.String))) //String Array Attribute
        {
            StringPopupAttribute attrib = (StringPopupAttribute)stringPopupAttributes[0];
            return attrib.DrawIntOrStringProperty(listenerTarget.objectReferenceValue, argument, argRect);
        }
        else return false;
    }

    public static int ProccessStringIntPair(int intValue, string stringValue, string[] optionsArray)
    {
        if (stringValue == null)
            return intValue;

        int[] equallyNamedOptions = optionsArray.Select((s, i) => new { i, s }).Where(t => t.s == stringValue).Select(t => t.i).ToArray();
        int result = (equallyNamedOptions.Length > 0) ? equallyNamedOptions[0] : -1;
        if (result < 0) return intValue;
        else return result;
    }

    public bool DrawIntOrStringProperty(SerializedProperty property,
        Rect argRect, bool drawWithLabel = false, GUIContent label = null)
    {
        object obj = GetSubObject(property.serializedObject.targetObject, property);
        return DrawIntOrStringProperty(obj, property, argRect, drawWithLabel, label);
    }

    public bool DrawIntOrStringProperty(object targetObj, SerializedProperty property,
        Rect argRect, bool drawWithLabel = false, GUIContent label = null)
    {
        string[] optionsArray = GetStringArray(targetObj);

        int sipIndex = -1;
        UnityEventPropertyIdentifier eventIdentifier = new UnityEventPropertyIdentifier(property.serializedObject.targetObject, property.propertyPath);
        StringPopupData[] popupDataArray = GetStringPopupData(targetObj, eventIdentifier, serializedPopupDataArray, propPath, ref sipIndex);

        if (PropertyIsValidForPopup(property, optionsArray))
        {
            //PrefabOverride rendering
            GUIStyle labelStyle = property.PrefabOverrideRendering(argRect);

            //Get intValue
            int intValue;
            if (property.propertyType == SerializedPropertyType.String)
            {
                if (property.stringValue == "") property.stringValue = optionsArray[0];
                int pastInt = (popupDataArray != null) ? popupDataArray[sipIndex].value : 0;
                intValue = ProccessStringIntPair(pastInt, property.stringValue, optionsArray);
            }
            else
            {
                string pastName = (popupDataArray != null) ? popupDataArray[sipIndex].text : null;
                intValue = ProccessStringIntPair(property.intValue, pastName, optionsArray);
            }

            //DropDown
            if (drawWithLabel)
            {
                Rect labelPosition = new Rect(argRect.x + EditorGUI.indentLevel, argRect.y, EditorGUIUtility.labelWidth - EditorGUI.indentLevel, argRect.height);
                Rect fieldPosition = new Rect(argRect.x + EditorGUIUtility.labelWidth + GUIInternalConstants.kPrefixPaddingRight,
                    argRect.y, argRect.width - EditorGUIUtility.labelWidth - GUIInternalConstants.kPrefixPaddingRight, argRect.height);

                EditorGUI.LabelField(labelPosition,
                    label == null ? new GUIContent(property.displayName) : label,
                    labelStyle);
                intValue = EditorGUI.Popup(fieldPosition, intValue, optionsArray);
            }
            else intValue = EditorGUI.Popup(argRect, intValue, optionsArray);
            string stringValue = optionsArray[intValue];

            //Set new value
            if (property.propertyType == SerializedPropertyType.String)
            {
                if (stringValue != property.stringValue)
                    property.stringValue = stringValue;
            }
            else
            {
                if (intValue != property.intValue)
                    property.intValue = Convert.ToInt32(intValue);
            }

            SetNewValue(targetObj, serializedPopupDataArray,
                popupDataArray, sipIndex, stringValue, intValue);
            return true;
        }
        else return false;
    }

    public void UpdateStringPopupData(Object targetObj, SerializedProperty property)
    {
        string[] optionsArray = GetStringArray(targetObj);

        string popupDataArrayName = serializedPopupDataArray;
        int sipIndex = -1;
        UnityEventPropertyIdentifier eventIdentifier = new UnityEventPropertyIdentifier(property.serializedObject.targetObject, property.propertyPath);
        StringPopupData[] popupDataArray = GetStringPopupData(targetObj, eventIdentifier, popupDataArrayName, propPath, ref sipIndex);

        if (PropertyIsValidForPopup(property, optionsArray) && (popupDataArray != null))
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                string str = property.stringValue;
                if (str == "")
                {
                    popupDataArray[sipIndex].text = property.stringValue = optionsArray[0];
                    popupDataArray[sipIndex].value = 0;
                }
                else
                {
                    popupDataArray[sipIndex].text = str;
                    popupDataArray[sipIndex].value = ProccessStringIntPair(-1, str, optionsArray);
                }
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                int value = property.intValue;
                popupDataArray[sipIndex].value = value;
                popupDataArray[sipIndex].text = optionsArray[value];
            }

            popupDataArray = popupDataArray.Where(x => x.sourceEventCall.sourceComponent != null).ToArray();
            ReflectionTools.SetValue(targetObj, serializedPopupDataArray, popupDataArray);
        }
    }

    public static int IntPopup(int current, object targetObj, StringPopupAttribute attr,
        string label, Rect argRect, SerializedProperty propertyPrefabOverrideData = null)
    {
        string[] optionsArray = attr.GetStringArray(targetObj);

        int sipIndex = -1;
        UnityEventPropertyIdentifier eventIdentifier = new UnityEventPropertyIdentifier((Object)targetObj, label);
        StringPopupData[] popupDataArray = GetStringPopupData(targetObj, eventIdentifier, attr.serializedPopupDataArray, attr.propPath, ref sipIndex);

        if ((!optionsArray.IsNullOrEmpty()) && (current >= 0) && (current < optionsArray.Length))
        {
            string pastName = (popupDataArray != null) ? popupDataArray[sipIndex].text : "";
            int intValue = ProccessStringIntPair(current, pastName, optionsArray);

            DrawPopup(argRect, optionsArray, intValue, label, propertyPrefabOverrideData);
            string stringValue = optionsArray[intValue];

            SetNewValue(targetObj, attr.serializedPopupDataArray,
                popupDataArray, sipIndex, stringValue, intValue);
            return intValue;
        }
        else return EditorGUI.IntField(argRect, label, current);
    }

    public static string StringPopup(string current, object targetObj, StringPopupAttribute attr,
        string label, Rect argRect, SerializedProperty propertyPrefabOverrideData = null)
    {
        string[] optionsArray = attr.GetStringArray(targetObj);

        int sipIndex = -1;
        UnityEventPropertyIdentifier eventIdentifier = new UnityEventPropertyIdentifier((Object)targetObj, label);
        StringPopupData[] popupDataArray = GetStringPopupData(targetObj, eventIdentifier, attr.serializedPopupDataArray, attr.propPath, ref sipIndex);

        if ((!optionsArray.IsNullOrEmpty()) && optionsArray.Contains(current))
        {
            int pastInt = (popupDataArray != null) ? popupDataArray[sipIndex].value : 0;
            int intValue = ProccessStringIntPair(pastInt, current, optionsArray);

            DrawPopup(argRect, optionsArray, intValue, label, propertyPrefabOverrideData);
            string stringValue = optionsArray[intValue];

            SetNewValue(targetObj, attr.serializedPopupDataArray,
                popupDataArray, sipIndex, stringValue, intValue);
            return stringValue;
        }
        else return EditorGUI.TextField(argRect, label, current);
    }

    static void SetNewValue(object targetObj, string serializedPopupDataArray, 
        StringPopupData[] popupDataArray, int sipIndex, string stringValue, int intValue)
    {
        if (popupDataArray != null)
        {
            popupDataArray[sipIndex].text = stringValue;
            popupDataArray[sipIndex].value = intValue;
            popupDataArray = popupDataArray.Where(x => x.sourceEventCall.sourceComponent != null).ToArray();
            ReflectionTools.SetValue(targetObj, serializedPopupDataArray, popupDataArray);
        }
    }

    static void DrawPopup(Rect rect, string[] optionsArray, int intValue, string label,
        SerializedProperty propertyPrefabOverrideData)
    {
        if (label != "")
        {
            GUIStyle labelStyle = propertyPrefabOverrideData.PrefabOverrideRendering(rect);
            Rect labelPosition = new Rect(rect.x + EditorGUI.indentLevel, rect.y, EditorGUIUtility.labelWidth - EditorGUI.indentLevel, rect.height);
            Rect fieldPosition = new Rect(rect.x + EditorGUIUtility.labelWidth + GUIInternalConstants.kPrefixPaddingRight,
                rect.y, rect.width - EditorGUIUtility.labelWidth - GUIInternalConstants.kPrefixPaddingRight, rect.height);

            EditorGUI.LabelField(labelPosition, label, labelStyle);
            intValue = EditorGUI.Popup(fieldPosition, intValue, optionsArray);
        }
        else intValue = EditorGUI.Popup(rect, intValue, optionsArray);
    }

    public string[] GetStringArray(object targetObj)
    {
        return GetValue<string[]>(targetObj);
    }

    #region Private utilities

    static StringPopupData[] GetStringPopupData(object targetObj, UnityEventPropertyIdentifier eventIdentifier, string dataArrayName, string stringArrayName, ref int index)
    {
        StringPopupData[] stringPopupDataArray = null;
        if (dataArrayName != "")
        {
            stringPopupDataArray = ReflectionTools.GetValue<StringPopupData[]>(targetObj, dataArrayName);
            if (stringPopupDataArray != null)
                for (int i = 0; i < stringPopupDataArray.Length; i++)
                    if (stringPopupDataArray[i].sourceEventCall.SameAs(eventIdentifier))
                    {
                        index = i;
                        break;
                    }
            if (index < 0)
            {
                StringPopupData newPair = new StringPopupData(eventIdentifier, "", stringArrayName, 0);
                if (stringPopupDataArray == null) stringPopupDataArray = new StringPopupData[] { newPair };
                else stringPopupDataArray = stringPopupDataArray.Append(newPair).ToArray();
                index = stringPopupDataArray.Length - 1;
            }
        }
        return stringPopupDataArray;
    }

    /// <summary>
    /// Checks if optionsArray is valid and string or int exists in the optionsArray.
    /// </summary>
    /// <param name="property"></param>
    /// <param name="optionsArray"></param>
    /// <returns></returns>
    static bool PropertyIsValidForPopup(SerializedProperty property, string[] optionsArray)
    {
        return ((optionsArray != null) && (optionsArray.Length > 0)) &&
                   (((property.propertyType == SerializedPropertyType.String) && (optionsArray.Contains(property.stringValue) || (property.stringValue == ""))) ||
                   ((property.propertyType == SerializedPropertyType.Integer) && ((property.intValue >= 0) && (property.intValue < optionsArray.Length))));
    }
    #endregion
#endif
}

#if UNITY_EDITOR
[Serializable]
public struct StringPopupData
//TO DO: Doesn't work, because two StringPopupData arrays can be pointing to the same event and it doesn't get corrected
//This could be solved by centralizing all StringPopupData arrays into one single dictionary, maybe a static variable.
//The problem with this is it must be serialized.
{
    public UnityEventPropertyIdentifier sourceEventCall;
    public string arrayName;
    public string text;
    public int value;

    public StringPopupData(UnityEventPropertyIdentifier sourceEventCall, string text, string arrayName, int value)
    {
        this.sourceEventCall = sourceEventCall;
        this.arrayName = arrayName;
        this.text = text;
        this.value = value;
    }

    public StringPopupData(string text, string arrayName, int value)
    {
        sourceEventCall = new UnityEventPropertyIdentifier(null, "");
        this.arrayName = arrayName;
        this.text = text;
        this.value = value;
    }

    public void SyncValues(string[] stringArray)
    {
        SerializedObject so = new SerializedObject(sourceEventCall.sourceComponent);
        SerializedProperty sp = so?.FindProperty(sourceEventCall.propertyPath);
        if (sp != null)
        {
            if (sp.propertyType == SerializedPropertyType.String)
            {
                value = StringPopupAttribute.ProccessStringIntPair(value, sp.stringValue, stringArray);
                if ((value >= 0f) && (value < stringArray.Length)) text = stringArray[value];
                else text = sp.stringValue;
                sp.stringValue = text;
            }
            else if (sp.propertyType == SerializedPropertyType.Integer)
            {
                value = StringPopupAttribute.ProccessStringIntPair(sp.intValue, text, stringArray);
                if ((value >= 0f) && (value < stringArray.Length)) text = stringArray[value];
                sp.intValue = value;
            }
            so.ApplyModifiedProperties();
        }
    }

    public static void CleanArrayOfNullValues(ref StringPopupData[] dataArray)
    {
        if (dataArray != null)
            dataArray = dataArray.Where(x => x.sourceEventCall.sourceComponent != null).ToArray();
    }

    public static void SyncArray(ref StringPopupData[] dataArray, object source)
    {
        if (dataArray != null)
        {
            CleanArrayOfNullValues(ref dataArray);
            for (int i = 0; i < dataArray.Length; i++)
            {
                string[] stringArray = ReflectionTools.GetValue<string[]>(source, dataArray[i].arrayName);
                dataArray[i].SyncValues(stringArray);
            }
        }
    }
}

[Serializable]
public struct UnityEventPropertyIdentifier
{
    public Object sourceComponent;
    public string propertyPath;

    public UnityEventPropertyIdentifier(Object sourceComponent, string propertyPath)
    {
        this.sourceComponent = sourceComponent;
        this.propertyPath = propertyPath;
    }

    public bool SameAs(UnityEventPropertyIdentifier other)
    {
        return (sourceComponent == other.sourceComponent) && (propertyPath == other.propertyPath);
    }
}

[CustomPropertyDrawer(typeof(StringPopupAttribute))]
public class StringPopupAttribute_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        StringPopupAttribute field = attribute as StringPopupAttribute;
        bool popupWasDrawn = field.DrawIntOrStringProperty(property, position, true, label);
        if (!popupWasDrawn) EditorGUI.PropertyField(position, property,
            new GUIContent(label.text + " *sp"));
    }
}
#endif