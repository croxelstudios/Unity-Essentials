using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif

[Serializable]
public class WeightedList<T> : BWeightedList
{
    [ListDrawerSettings(CustomAddFunction = "AddListElement", CustomRemoveElementFunction = "Remove", CustomRemoveIndexFunction = "RemoveAt")]
    public WeightedObject[] elements;
    public int Length { get { return elements.Length; } }

    public WeightedList(params T[] elements)
    {
        this.elements = new WeightedObject[elements.Length];
        for (int i = 0; i < this.elements.Length; i++)
            this.elements[i] = new WeightedObject(elements[i], 1f);
    }

    public WeightedList(WeightedList<T> elements)
    {
        this.elements = new WeightedObject[0];
        Add(elements);
    }

    public float GetTotalWeight()
    {
        float totalWeight = 0f;
        for (int i = 0; i < elements.Length; i++) totalWeight += elements[i].weight;
        return totalWeight;
    }

    public T ChooseWeightedElement()
    {
        return elements[ChooseWeightedElementID()].element;
    }

    public int ChooseWeightedElementID()
    {
        float totalWeight = GetTotalWeight();

        int chosen = -1;
        float currentWeight = Random.Range(0, totalWeight);
        for (int i = 0; i < elements.Length; i++)
        {
            if (currentWeight <= elements[i].weight)
            {
                chosen = i;
                break;
            }
            else currentWeight -= elements[i].weight;
        }

        return chosen;
    }

    public T GetElementFromID(int id)
    {
        return elements[id].element;
    }

    public float GetWeightFromID(int id)
    {
        return elements[id].weight;
    }

    public virtual WeightedObject Add(T element)
    {
        return Add(element, 1f);
    }

    public virtual WeightedObject Add(T element, float weight)
    {
        WeightedObject obj = new WeightedObject(element, weight);
        elements = elements.Append(obj).ToArray();
        return obj;
    }

    public virtual void Add(WeightedList<T> elements)
    {
        for (int i = 0; i < elements.Length; i++)
            Add(elements.GetElementFromID(i), elements.GetWeightFromID(i));
    }

    public virtual void Remove(T element)
    {
        elements = elements.Where(x => !x.element.Equals(element)).ToArray();
    }

    public virtual bool Contains(T element)
    {
        return elements.Where(x => x.element.Equals(element)).Count() > 0;
    }

    public virtual void RemoveAt(int id)
    {
        elements = elements.Where(x => !x.Equals(elements[id])).ToArray();
    }

    protected virtual void AddListElement()
    {
        Add(default(T));
    }

    [Serializable]
    public class WeightedObject
    {
        public T element;
        public float weight;
        public int hash;

        public WeightedObject(T element, float weight)
        {
            this.element = element;
            this.weight = weight;
            hash = this.GetHashCode();
        }
    }
}

public class BWeightedList
{
}

#if UNITY_EDITOR
/*
[CustomPropertyDrawer(typeof(WeightedList<>), true)]
public class WeightedListDrawer : PropertyDrawer
{
    SerializedObject targetObject;
    SerializedProperty thisProperty;
    SerializedProperty elements;
    ReorderableList list;
    GUIContent globalLabel;

    const string elementsName = "elements";
    const string elementName = "element";
    const string weightName = "weight";

    const string elementDisplayName = "Element";
    const string weightDisplayName = "Weight";

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        globalLabel = label;
        if (thisProperty == null) thisProperty = property;
        if (targetObject == null) targetObject = property.serializedObject;
        if (elements == null) elements = property.FindPropertyRelative(elementsName);

        if (list == null)
        {
            list = new ReorderableList(property.serializedObject, elements, true, true, true, true)
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

        return 0f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        if (!property.isExpanded) property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
        else DoLayoutList();
        property.serializedObject.ApplyModifiedProperties();
        EditorGUI.EndProperty();
    }

    protected void DoLayoutList()
    {
        list.DoLayoutList();
    }

    #region ReorderableList callbacks
    void DrawListHeader(Rect rect)
    {
        Rect newRect = new Rect(rect.x - 7f, rect.y, rect.width + 7f, rect.height);
        thisProperty.isExpanded = EditorGUI.Foldout(newRect, thisProperty.isExpanded, new GUIContent(" " + globalLabel.text), true);
    }

    private float ElementHeightCallback(int index)
    {
        SerializedProperty weightedElement = list.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty element = weightedElement.FindPropertyRelative(elementName);
        SerializedProperty weight = weightedElement.FindPropertyRelative(weightName);
        float height = EditorGUI.GetPropertyHeight(element, true);
        height += EditorGUI.GetPropertyHeight(weight);
        return height;
    }

    void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty weightedElement = list.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty element = weightedElement.FindPropertyRelative(elementName);
        SerializedProperty weight = weightedElement.FindPropertyRelative(weightName);

        float y = rect.y;

        element.PropertyField(elementDisplayName, y, rect);
        y += EditorGUI.GetPropertyHeight(element, true);

        weight.PropertyField(weightDisplayName, y, rect);
    }

    void OnSelectElement(ReorderableList list)
    {
    }

    void OnReorderList(ReorderableList list)
    {
        targetObject.ApplyModifiedProperties();
    }

    void OnAddElement(ReorderableList list)
    {
        new ReorderableList.Defaults().DoAddButton(list);

        SerializedProperty weightedElement = list.serializedProperty.GetArrayElementAtIndex(list.index);
        SerializedProperty weight = weightedElement.FindPropertyRelative(weightName);
        weight.floatValue = 1f;

        targetObject.ApplyModifiedProperties();
    }

    void OnRemoveElement(ReorderableList list)
    {
        new ReorderableList.Defaults().DoRemoveButton(list);
        targetObject.ApplyModifiedProperties();
    }
    #endregion
}
*/

public class WeightedListDrawer<WL, T> : OdinValueDrawer<WL> where WL : WeightedList<T>
{
    private InspectorProperty elementsProp;

    protected override void Initialize()
    {
        elementsProp = Property.Children["elements"];
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        WeightedObjectDrawerHelper.WLProperty = ValueEntry.Property;
        elementsProp.Draw();
    }
}

public class WeightedObjectDrawer<T> : OdinValueDrawer<WeightedList<T>.WeightedObject>
{
    private InspectorProperty elementProp;
    private InspectorProperty weightProp;

    protected override void Initialize()
    {
        elementProp = Property.Children["element"];
        weightProp = Property.Children["weight"];
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        bool removeFoldout = WeightedObjectDrawerHelper.WLProperty?.GetAttribute<RemoveFoldoutAttribute>() != null;
        WeightedList_Percent<T> wlp = WeightedObjectDrawerHelper.WLProperty.ValueEntry.WeakSmartValue as WeightedList_Percent<T>;

        DrawElementsProp(elementProp, removeFoldout);
        if (wlp != null)
        {
            EditorGUI.BeginChangeCheck();
            weightProp.ValueEntry.WeakSmartValue = EditorGUILayout.Slider(weightProp.NiceName, (float)weightProp.ValueEntry.WeakSmartValue, 0f, 100f);
            if (EditorGUI.EndChangeCheck())
            {
                weightProp.ValueEntry.ApplyChanges();
                wlp.UpdateDistribution(ValueEntry.SmartValue);
            }
            wlp.SetLock(ValueEntry.SmartValue, EditorGUILayout.Toggle("Lock", wlp.GetLock(ValueEntry.SmartValue)));
        }
        else weightProp.Draw();
    }

    void DrawElementsProp(InspectorProperty elementProp, bool removeFoldout)
    {
        if (removeFoldout) RemoveFoldoutAttribute.DrawSubfields(elementProp);
        else elementProp.Draw();
    }
}

public static class WeightedObjectDrawerHelper
{
    public static InspectorProperty WLProperty;
}
#endif
