using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif

[Serializable]
public class WeightedList_Percent<T> : WeightedList<T>
{
    [SerializeField]
    LocksDictionary locks;

    [Serializable]
    class LocksDictionary : SerializableDictionary<int, bool> { }

    public WeightedList_Percent(params T[] elements)
    {
        this.elements = new WeightedObject[elements.Length];
        decimal sameWeight = 100.000m / elements.Length;
        locks = new LocksDictionary();
        for (int i = 0; i < this.elements.Length; i++)
        {
            this.elements[i].element = elements[i];
            this.elements[i].weight = (float)sameWeight;
            locks.Add(this.elements[i].hash, false);
        }
    }

    public WeightedList_Percent(WeightedList<T> elements)
    {
        this.elements = new WeightedObject[0];
        Add(elements);
    }

    public WeightedList_Percent(WeightedList_Percent<T> elements)
    {
        this.elements = new WeightedObject[0];
        Add(elements);
    }

    public bool GetLock(WeightedObject obj)
    {
        return locks[obj.hash];
    }

    public void SetLock(WeightedObject obj, bool value)
    {
        locks[obj.hash] = value;
    }

    public bool GetLock(int id)
    {
        return GetLock(elements[id]);
    }

    public override WeightedObject Add(T element)
    {
        decimal sameWeight = 100.000m / (elements.Length + 1);
        return Add(element, (float)sameWeight);
    }

    public override WeightedObject Add(T element, float weight)
    {
        WeightedObject obj = base.Add(element, weight);
        AddLock(obj);
        UpdateDistribution(obj);
        return obj;
    }

    public override void Add(WeightedList<T> elements)
    {
        float weightMult = GetWeightToPercetMult(elements.GetTotalWeight());
        for (int i = 0; i < elements.Length; i++)
            Add(elements.GetElementFromID(i), elements.GetWeightFromID(i) * weightMult * 0.5f);
    }

    public void Add(T element, bool _lock)
    {
        WeightedObject obj = Add(element);
        SetLock(obj, _lock);
    }

    public void Add(T element, float weight, bool _lock)
    {
        WeightedObject obj = Add(element, weight);
        SetLock(obj, _lock);
    }

    public void Add(WeightedList_Percent<T> elements)
    {
        for (int i = 0; i < elements.Length; i++)
            Add(elements.GetElementFromID(i), elements.GetWeightFromID(i) * 0.5f, elements.GetLock(i));
    }

    public override void Remove(T element)
    {
        IEnumerable<WeightedObject> foundElements = elements.Where(x => x.element.Equals(element));
        List<int> foundHashes = elements.Select(x => x.hash).ToList();
        locks = (LocksDictionary)locks.Where(x => !foundHashes.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        base.Remove(element);
        UpdateDistribution();
    }

    public override void RemoveAt(int id)
    {
        locks.Remove(elements[id].hash);
        base.RemoveAt(id);
        UpdateDistribution();
    }

    public void UpdateDistribution(params WeightedObject[] exceptions)
    {
        List<int> unlocked = new List<int>();
        decimal totalWeight = 0.000m;
        decimal totalUnlockedWeight = 0.000m;
        for (int i = 0; i < elements.Length; i++)
        {
            totalWeight += (decimal)elements[i].weight;
            if (!exceptions.Contains(elements[i]))
            {
                if (!GetLock(i))
                {
                    unlocked.Add(i);
                    totalUnlockedWeight += (decimal)elements[i].weight;
                }
            }
        }

        decimal change = 100.000m - totalWeight;
        decimal totalChange = 0.000m;
        if (unlocked.Count > 0)
        {
            decimal microChange = change / unlocked.Count;
            for (int i = 0; i < unlocked.Count; i++)
            {
                decimal localChange;
                if (totalUnlockedWeight > 0.000m)
                    localChange = change * (decimal)elements[unlocked[i]].weight / totalUnlockedWeight;
                else localChange = microChange;

                if (((decimal)elements[unlocked[i]].weight + localChange) < 0.000m)
                    localChange = -(decimal)elements[unlocked[i]].weight;

                totalChange += localChange;
                elements[unlocked[i]].weight += (float)localChange;
            }
        }
        decimal excess = change - totalChange;
        if ((exceptions.Length > 0) && (Mathf.Abs((float)(excess)) > 0f))
        {
            decimal microExcess = excess / exceptions.Length;
            for (int i = 0; i < exceptions.Length; i++)
                exceptions[i].weight += (float)microExcess;
        }
    }

    void AddLock(WeightedObject obj)
    {
        CheckLock(obj);
        SetLock(obj, false);
    }

    void CheckLock(WeightedObject obj)
    {
        if (locks == null) locks = new LocksDictionary();
        if (!locks.ContainsKey(obj.hash)) locks.Add(obj.hash, false);
    }

    float GetWeightToPercetMult(float totalweight)
    {
        return 100f / totalweight;
    }
}


#if UNITY_EDITOR
/*
[CustomPropertyDrawer(typeof(WeightedList_Percent<>), true)]
public class DistributedListDrawer : PropertyDrawer
{
    SerializedObject targetObject;
    SerializedProperty thisProperty;
    SerializedProperty elements;
    ReorderableList list;
    GUIContent globalLabel;

    const string elementsName = "elements";
    const string elementName = "element";
    const string weightName = "weight";
    const string lockName = "_lock";

    const string elementDisplayName = "Element";
    const string weightDisplayName = "Weight";
    const string lockDisplayName = "Lock";

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

    void UpdateDistribution(params int[] exceptionIndex)
    {
        List<SerializedProperty> unlocked = new List<SerializedProperty>();
        decimal totalWeight = 0.000m;
        decimal totalUnlockedWeight = 0.000m;
        for (int i = 0; i < list.serializedProperty.arraySize; i++)
        {
            SerializedProperty weightedElement = list.serializedProperty.GetArrayElementAtIndex(i);
            SerializedProperty weight = weightedElement.FindPropertyRelative(weightName);
            totalWeight += (decimal)weight.floatValue;
            if (!exceptionIndex.Contains(i))
            {
                SerializedProperty _lock = weightedElement.FindPropertyRelative(lockName);
                if (!_lock.boolValue)
                {
                    unlocked.Add(weightedElement);
                    totalUnlockedWeight += (decimal)weight.floatValue;
                }
            }
        }

        decimal change = 100.000m - totalWeight;
        decimal totalChange = 0.000m;
        if (unlocked.Count > 0)
        {
            decimal microChange = change / unlocked.Count;
            for (int i = 0; i < unlocked.Count; i++)
            {
                SerializedProperty weight = unlocked[i].FindPropertyRelative(weightName);

                decimal localChange;
                if (totalUnlockedWeight > 0.000m)
                    localChange = change * (decimal)weight.floatValue / totalUnlockedWeight;
                else localChange = microChange;

                if (((decimal)weight.floatValue + localChange) < 0.000m)
                    localChange = -(decimal)weight.floatValue;

                totalChange += localChange;
                weight.floatValue += (float)localChange;
            }
        }
        decimal excess = change - totalChange;
        if (Mathf.Abs((float)(excess)) > 0f)
        {
            decimal microExcess = excess / exceptionIndex.Length;
            for (int i = 0; i < exceptionIndex.Length; i++)
            {
                SerializedProperty weightedElement = list.serializedProperty.GetArrayElementAtIndex(exceptionIndex[i]);
                SerializedProperty weight = weightedElement.FindPropertyRelative(weightName);
                weight.floatValue += (float)microExcess;
            }
        }
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
        SerializedProperty _lock = weightedElement.FindPropertyRelative(lockName);
        float height = EditorGUI.GetPropertyHeight(element, true);
        height += EditorGUI.GetPropertyHeight(weight);
        height += EditorGUI.GetPropertyHeight(_lock);
        return height;
    }

    void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty weightedElement = list.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty element = weightedElement.FindPropertyRelative(elementName);
        SerializedProperty weight = weightedElement.FindPropertyRelative(weightName);
        SerializedProperty _lock = weightedElement.FindPropertyRelative(lockName);

        float y = rect.y;

        element.PropertyField(elementDisplayName, y, rect);
        y += EditorGUI.GetPropertyHeight(element, true);

        EditorGUI.BeginChangeCheck();
        weight.PropertyField(weightDisplayName, y, rect);
        if (EditorGUI.EndChangeCheck()) UpdateDistribution(index);
        y += EditorGUI.GetPropertyHeight(weight);

        _lock.PropertyField(lockDisplayName, y, rect);
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
        if (list.serializedProperty.arraySize <= 1)
            weight.floatValue = 100f;
        else weight.floatValue = 0f;

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
#endif
