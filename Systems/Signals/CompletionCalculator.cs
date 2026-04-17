using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(100)]
public class CompletionCalculator : MonoBehaviour
{
    [SerializeField]
    WeightedList<FloatCompletion> floats = null;
    [SerializeField]
    WeightedList<IntCompletion> ints = null;
    [SerializeField]
    WeightedList<StringListCompletion> registers = null;
    [SerializeField]
    WeightedList<Flag> flags = null;
    [SerializeField]
    DXFloatEvent current = null;
    [SerializeField]
    bool checkCompletionOnEnable = false;
    [SerializeField]
    DXEvent completed = null;
    [SerializeField]
    bool checkDepletionOnEnable = false;
    [SerializeField]
    DXEvent depleted = null;

    float completion;

    void OnEnable()
    {
        StartCoroutine(Init());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator Init()
    {
        yield return new WaitForEndOfFrame();
        completion = Calculate();
        current?.Invoke(completion);
        if (checkCompletionOnEnable)
            CheckCompletion();
        if (checkDepletionOnEnable)
            CheckDepletion();

        foreach (WeightedList<StringListCompletion>.WeightedObject obj in registers.elements)
            obj.element.SubscribeOnChange(CalculateAndCheck);
        foreach (WeightedList<FloatCompletion>.WeightedObject obj in floats.elements)
            obj.element.SubscribeOnChange(CalculateAndCheck);
        foreach (WeightedList<IntCompletion>.WeightedObject obj in ints.elements)
            obj.element.SubscribeOnChange(CalculateAndCheck);
        foreach (WeightedList<Flag>.WeightedObject obj in flags.elements)
            obj.element.AddListener(CalculateAndCheck);
    }

    void CalculateAndCheck()
    {
        float last = completion;
        completion = Calculate();
        current?.Invoke(completion);
        if (last < 1f)
            CheckCompletion();
        if (last > 0f)
            CheckDepletion();
    }

    float Calculate()
    {
        float completion = 0f;
        float max = 0f;
        foreach (WeightedList<FloatCompletion>.WeightedObject obj in floats.elements)
        {
            completion += obj.element.current * obj.weight;
            max += obj.element.Max * obj.weight;
        }
        foreach (WeightedList<IntCompletion>.WeightedObject obj in ints.elements)
        {
            completion += obj.element.current * obj.weight;
            max += obj.element.Max * obj.weight;
        }
        foreach (WeightedList<StringListCompletion>.WeightedObject obj in registers.elements)
        {
            completion += obj.element.current * obj.weight;
            max += obj.element.Max * obj.weight;
        }
        foreach (WeightedList<Flag>.WeightedObject obj in flags.elements)
        {
            completion += obj.element.currentValue ? obj.weight : 0f;
            max += obj.weight;
        }

        if (max > 0f)
            return completion / max;
        else return 0f;
    }

    void CheckCompletion()
    {
        if (completion >= 1f)
            completed?.Invoke();
    }

    void CheckDepletion()
    {
        if (completion <= 0f)
            depleted?.Invoke();
    }

    [Serializable]
    [HideLabel]
    [InlineProperty]
    struct StringListCompletion
    {
        [SerializeField]
        [HorizontalGroup]
        StringList count;
        [SerializeField]
        [HorizontalGroup]
        float max;
        public float current { get { return Mathf.Min(count.Count, Max); } }
        public float Max { get { return max; } }
        DXEvent change;

        public StringListCompletion(StringList count, float max)
        {
            this.count = count;
            this.max = max;
            change = new DXEvent();
        }

        public void SubscribeOnChange(UnityAction action)
        {
            count.valueAdded.RemoveListener(Changed);
            count.valueAdded.AddListener(Changed);
            count.valueRemoved.RemoveListener(Changed);
            count.valueRemoved.AddListener(Changed);
            if (change == null)
                change = new DXEvent();
            change.AddListener(action);
        }

        void Changed(string value)
        {
            change?.Invoke();
        }
    }

    [Serializable]
    [HideLabel]
    [InlineProperty]
    struct FloatCompletion
    {
        [SerializeField]
        [HorizontalGroup]
        FloatSignal count;
        [SerializeField]
        [HorizontalGroup]
        FloatVariable max;
        public float current { get { return Mathf.Min(count.currentValue, Max); } }
        public float Max { get { return max.Value; } }

        public FloatCompletion(FloatSignal count, FloatVariable max)
        {
            this.count = count;
            this.max = max;
        }

        public void SubscribeOnChange(UnityAction action)
        {
            count.AddListener(action);
            max.TrySubscribeOnChange(action);
        }
    }

    [Serializable]
    [HideLabel]
    [InlineProperty]
    struct IntCompletion
    {
        [SerializeField]
        [HorizontalGroup]
        IntSignal count;
        [SerializeField]
        [HorizontalGroup]
        IntVariable max;
        public float current { get { return Mathf.Min(count.currentValue, Max); } }
        public float Max { get { return max.Value; } }

        public IntCompletion(IntSignal count, IntVariable max)
        {
            this.count = count;
            this.max = max;
        }

        public void SubscribeOnChange(UnityAction action)
        {
            count.AddListener(action);
            max.TrySubscribeOnChange(action);
        }
    }

    [Serializable]
    [HideLabel]
    [InlineProperty]
    struct FloatVariable
    {
        [SerializeField]
        [HideLabel]
        [ShowIf("fromSignal")]
        [HorizontalGroup]
        FloatSignal maxSignal;
        [SerializeField]
        [HideLabel]
        [HideIf("fromSignal")]
        [HorizontalGroup]
        float maxValue;
        [SerializeField]
        [HorizontalGroup]
        bool fromSignal;

        public FloatVariable(FloatSignal maxSignal, float maxValue, bool fromSignal)
        {
            this.maxSignal = maxSignal;
            this.maxValue = maxValue;
            this.fromSignal = fromSignal;
        }

        public float Value => fromSignal ? maxSignal.currentValue : maxValue;

        public void TrySubscribeOnChange(UnityAction action)
        {
            if (fromSignal)
                maxSignal.AddListener(action);
        }
    }

    [Serializable]
    [HideLabel]
    [InlineProperty]
    struct IntVariable
    {
        [SerializeField]
        [HideLabel]
        [ShowIf("fromSignal")]
        [HorizontalGroup]
        IntSignal maxSignal;
        [SerializeField]
        [HideLabel]
        [HideIf("fromSignal")]
        [HorizontalGroup]
        int maxValue;
        [SerializeField]
        [HorizontalGroup]
        bool fromSignal;

        public IntVariable(IntSignal maxSignal, int maxValue, bool fromSignal)
        {
            this.maxSignal = maxSignal;
            this.maxValue = maxValue;
            this.fromSignal = fromSignal;
        }

        public int Value => fromSignal ? maxSignal.currentValue : maxValue;

        public void TrySubscribeOnChange(UnityAction action)
        {
            if (fromSignal)
                maxSignal.AddListener(action);
        }
    }
}
