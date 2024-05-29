using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using System.Linq;
using Object = UnityEngine.Object;
#endif

/// <summary>
/// Mark a method with an integer argument with this to display the argument as an enum popup in the UnityEvent
/// drawer. Use: [EnumAction(typeof(SomeEnumType))]
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class StringPopupAttribute : PropertyAttribute
{
    public string serializedPopupDataArray;
    public string[] optionsArrayPath;

    public StringPopupAttribute(string optionsArrayName)
    {
        optionsArrayPath = new string[] { optionsArrayName };
        serializedPopupDataArray = "";
    }

    public StringPopupAttribute(string[] optionsArrayPath)
    {
        serializedPopupDataArray = "";
        this.optionsArrayPath = optionsArrayPath;
    }

    public StringPopupAttribute(string optionsArrayName, string serializedPopupDataArray)
    {
        optionsArrayPath = new string[] { optionsArrayName };
        this.serializedPopupDataArray = serializedPopupDataArray;
    }

    public StringPopupAttribute(string[] optionsArrayPath, string serializedPopupDataArray)
    {
        this.serializedPopupDataArray = serializedPopupDataArray;
        this.optionsArrayPath = optionsArrayPath;
    }

#if UNITY_EDITOR
    public static bool InterpretInEventsDrawer(MethodInfo method, SerializedProperty argument, Rect argRect, SerializedProperty listenerTarget)
    {
        object[] stringPopupAttributes = null;
        if (method != null) stringPopupAttributes = method.GetCustomAttributes(typeof(StringPopupAttribute), true);
        if ((stringPopupAttributes != null) && (stringPopupAttributes.Length > 0) &&
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
        int[] equallyNamedOptions = optionsArray.Select((s, i) => new { i, s }).Where(t => t.s == stringValue).Select(t => t.i).ToArray();
        int result = (equallyNamedOptions.Length > 0) ? equallyNamedOptions[0] : -1;
        if (result < 0) return intValue;
        else return result;
    }

    public bool DrawIntOrStringProperty(object targetObj, SerializedProperty property, Rect argRect, bool drawWithLabel = false)
    {
        string[] optionsArray = GetStringArray(targetObj);

        int sipIndex = -1;
        UnityEventPropertyIdentifier eventIdentifier = new UnityEventPropertyIdentifier(property.serializedObject.targetObject, property.propertyPath);
        //Won't work with paths now
        StringPopupData[] popupDataArray = GetStringPopupData(targetObj, eventIdentifier, serializedPopupDataArray, optionsArrayPath[0], ref sipIndex);

        if (PropertyIsValidForPopup(property, optionsArray))
        {
            //PrefabOverride rendering
            GUIStyle labelStyle = PrefabOverrideRendering(argRect, property);

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
                string pastName = (popupDataArray != null) ? popupDataArray[sipIndex].text : "";
                intValue = ProccessStringIntPair(property.intValue, pastName, optionsArray);
            }

            //DropDown
            if (drawWithLabel)
            {
                Rect labelPosition = new Rect(argRect.x + EditorGUI.indentLevel, argRect.y, EditorGUIUtility.labelWidth - EditorGUI.indentLevel, argRect.height);
                Rect fieldPosition = new Rect(argRect.x + EditorGUIUtility.labelWidth + GUIInternalConstants.kPrefixPaddingRight,
                    argRect.y, argRect.width - EditorGUIUtility.labelWidth - GUIInternalConstants.kPrefixPaddingRight, argRect.height);

                EditorGUI.LabelField(labelPosition, property.displayName, labelStyle);
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

            if (popupDataArray != null)
            {
                popupDataArray[sipIndex].text = stringValue;
                popupDataArray[sipIndex].value = intValue;
                popupDataArray = popupDataArray.Where(x => x.sourceEventCall.sourceComponent != null).ToArray();
                ReflectionTools.SetValue(targetObj, serializedPopupDataArray, popupDataArray);
            }
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
        //Won't work with paths now
        StringPopupData[] popupDataArray = GetStringPopupData(targetObj, eventIdentifier, popupDataArrayName, optionsArrayPath[0], ref sipIndex);

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
    public object GetSubObject(Object targetObj, SerializedProperty property)
    {
        string[] pathSegments = property.propertyPath.Split(".");
        string path = "";
        int stop;
        if ((pathSegments.Length > 1) && (pathSegments[pathSegments.Length - 2] == "Array"))
            stop = pathSegments.Length - 3;
        else stop = pathSegments.Length - 1;
        for (int i = 0; i < stop; i++)
        {
            if (i != 0) path += ".";
            path += pathSegments[i];
        }
        return (path != "") ? ReflectionTools.GetFieldValue(targetObj, path) : targetObj;
    }

    public static int IntPopup(int current, object targetObj, StringPopupAttribute attr,
        string label, Rect argRect, SerializedProperty propertyPrefabOverrideData = null)
    {
        string[] optionsArray = attr.GetStringArray(targetObj);

        int sipIndex = -1;
        UnityEventPropertyIdentifier eventIdentifier = new UnityEventPropertyIdentifier((Object)targetObj, label);
        //Won't work with paths now
        StringPopupData[] popupDataArray = GetStringPopupData(targetObj, eventIdentifier, attr.serializedPopupDataArray, attr.optionsArrayPath[0], ref sipIndex);

        if ((optionsArray != null) && (optionsArray.Length > 0) && (current >= 0) && (current < optionsArray.Length))
        {
            string pastName = (popupDataArray != null) ? popupDataArray[sipIndex].text : "";
            int intValue = ProccessStringIntPair(current, pastName, optionsArray);

            //DropDown
            if (label != "")
            {
                GUIStyle labelStyle = PrefabOverrideRendering(argRect, propertyPrefabOverrideData);
                Rect labelPosition = new Rect(argRect.x + EditorGUI.indentLevel, argRect.y, EditorGUIUtility.labelWidth - EditorGUI.indentLevel, argRect.height);
                Rect fieldPosition = new Rect(argRect.x + EditorGUIUtility.labelWidth + GUIInternalConstants.kPrefixPaddingRight,
                    argRect.y, argRect.width - EditorGUIUtility.labelWidth - GUIInternalConstants.kPrefixPaddingRight, argRect.height);

                EditorGUI.LabelField(labelPosition, label, labelStyle);
                intValue = EditorGUI.Popup(fieldPosition, intValue, optionsArray);
            }
            else intValue = EditorGUI.Popup(argRect, intValue, optionsArray);
            string stringValue = optionsArray[intValue];

            //Set new value
            if (popupDataArray != null)
            {
                popupDataArray[sipIndex].text = stringValue;
                popupDataArray[sipIndex].value = intValue;
                popupDataArray = popupDataArray.Where(x => x.sourceEventCall.sourceComponent != null).ToArray();
                ReflectionTools.SetValue(targetObj, attr.serializedPopupDataArray, popupDataArray);
            }
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
        //Won't work with paths now
        StringPopupData[] popupDataArray = GetStringPopupData(targetObj, eventIdentifier, attr.serializedPopupDataArray, attr.optionsArrayPath[0], ref sipIndex);

        if ((optionsArray != null) && (optionsArray.Length > 0) && optionsArray.Contains(current))
        {
            int pastInt = (popupDataArray != null) ? popupDataArray[sipIndex].value : 0;
            int intValue = ProccessStringIntPair(pastInt, current, optionsArray);

            //DropDown
            if (label != "")
            {
                GUIStyle labelStyle = PrefabOverrideRendering(argRect, propertyPrefabOverrideData);
                Rect labelPosition = new Rect(argRect.x + EditorGUI.indentLevel, argRect.y, EditorGUIUtility.labelWidth - EditorGUI.indentLevel, argRect.height);
                Rect fieldPosition = new Rect(argRect.x + EditorGUIUtility.labelWidth + GUIInternalConstants.kPrefixPaddingRight,
                    argRect.y, argRect.width - EditorGUIUtility.labelWidth - GUIInternalConstants.kPrefixPaddingRight, argRect.height);

                EditorGUI.LabelField(labelPosition, label, labelStyle);
                intValue = EditorGUI.Popup(fieldPosition, intValue, optionsArray);
            }
            else intValue = EditorGUI.Popup(argRect, intValue, optionsArray);
            string stringValue = optionsArray[intValue];

            //Set new value
            if (popupDataArray != null)
            {
                popupDataArray[sipIndex].text = stringValue;
                popupDataArray[sipIndex].value = intValue;
                popupDataArray = popupDataArray.Where(x => x.sourceEventCall.sourceComponent != null).ToArray();
                ReflectionTools.SetValue(targetObj, attr.serializedPopupDataArray, popupDataArray);
            }
            return stringValue;
        }
        else return EditorGUI.TextField(argRect, label, current);
    }

    public string[] GetStringArray(object targetObj)
    {
        if (optionsArrayPath != null)
        {
            object obj = targetObj;
            for (int i = 0; i < optionsArrayPath.Length; i++)
            {
                if (obj != null) obj = ReflectionTools.GetFieldValue(obj, optionsArrayPath[i]);
                else break;
            }
            string[] result = obj as string[];
            if (result == null)
            {
                EventNamesData namesData = obj as EventNamesData;
                if (namesData != null) result = namesData.names;
            }
            return result;
        }
        else return null;
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

    static GUIStyle PrefabOverrideRendering(Rect argRect, SerializedProperty property)
    {
        GUIStyle labelStyle = EditorStyles.label;
        if (property != null)
        {
            Object[] objs = property.serializedObject.targetObjects;
            if (objs.Length == 1) //Prefab override rendering
            {
                bool hasPrefabOverride = property.prefabOverride;
                //if (!linkedProperties || hasPrefabOverride)
                //    EditorGUIUtility.SetBoldDefaultFont(hasPrefabOverride);
                if (hasPrefabOverride && !property.isDefaultOverride/* && !property.isDrivenRectTransformProperty*/)
                {
                    Rect highlightRect = argRect;
                    highlightRect.xMin += EditorGUI.indentLevel;

                    highlightRect.x = 0f;
                    highlightRect.width = 2;
                    Graphics.DrawTexture(highlightRect, EditorGUIUtility.whiteTexture,
                        new Rect(), 0, 0, 0, 0, GUIInternalConstants.prefabOverrideColor,
                        new Material(EditorGUIUtility.LoadRequired(GUIInternalConstants.prefabOverrideShaderPath) as Shader));
                    labelStyle = EditorStyles.boldLabel;
                }
            }
        }
        return labelStyle;
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
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) //TO DO: Maybe use this label? could be an argument for DrawIntOrStringProperty
    {
        StringPopupAttribute field = attribute as StringPopupAttribute;
        object obj = field.GetSubObject(property.serializedObject.targetObject, property);
        bool popupWasDrawn = field.DrawIntOrStringProperty(obj, property, position, true);
        if (!popupWasDrawn) EditorGUI.PropertyField(position, property, new GUIContent(label.text + " *sp"));
    }
}
#endif