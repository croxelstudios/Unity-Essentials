using System;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class IntHolder : MonoBehaviour
{
    public int current = 0;
    [SerializeField]
    bool launchCurrentOnEnable = true;
    public DXIntEvent intChanged = null;
    [SerializeField]
    [FoldoutGroup("On call event")]
    DXIntEvent intCalled = null;
    [SerializeField]
    IntHolder originIntHolder = null;
    [Space]
    [SerializeField]
    OnNumberEvent[] onNumberEvents = null;
    [SerializeField]
    OnVariationEvent[] onAdditionEvents = null;
    [SerializeField]
    OnVariationEvent[] onReductionEvents = null;
    [SerializeField]
    CountMode countMode = CountMode.Infinite;
    [SerializeField]
    [HideIf("countMode", CountMode.Infinite)]
    Vector2Int limits = new Vector2Int(0, 5);

    [SerializeField]
    [HideIf("countMode", CountMode.Infinite)]
    DXEvent onTopLimitReached = null;
    [SerializeField]
    [HideIf("countMode", CountMode.Infinite)]
    DXEvent onBottomLimitReached = null;

    [SerializeField]
    bool resetOnDisable = false;

    int pingPongValue;

    protected virtual void OnEnable()
    {
        pingPongValue = current;
        if (originIntHolder != null)
        {
            Set(originIntHolder.current);
            originIntHolder.intChanged =
                originIntHolder.intChanged.CreateAddListener<DXIntEvent, int>(Set);
        }
        if(launchCurrentOnEnable)
            ForceManageIntChangeGeneric(current, true);
    }

    protected virtual void OnDisable()
    {
        if (originIntHolder != null)
            originIntHolder.intChanged.SmartRemoveListener<DXIntEvent, int>(Set);
        ForceManageIntChangeGeneric(current, false);
        if (resetOnDisable) ResetHolder();
    }

    public void SendInt()
    {
        intCalled?.Invoke(current);
    }

    public void Set(float value)
    {
        Set(Mathf.FloorToInt(value));
    }

    public void Set(int value)
    {
        Add(value - current);
    }

    public void Add()
    {
        Add(1);
    }

    public void Add(int amount)
    {
        if (this.IsActiveAndEnabled())
        {
            int prev = current;
            current = AddWithCountMode(current, amount, limits, countMode);
            int dif = current - prev;
            if (dif > 0)
            {
                ManageIntChangeGeneric(prev, current);
                ManageIntAddition(prev, current);
            }
            else if (dif < 0)
            {
                ManageIntChangeGeneric(prev, current);
                ManageIntSubtraction(prev, current);
            }
        }
    }

    public void Subtract()
    {
        Subtract(1);
    }

    public void Subtract(int amount)
    {
        Add(-amount);
    }

    public void ResetHolder()
    {
        current = 0;
    }

    void ManageIntAddition(int prev, int current)
    {
        onAdditionEvents = onAdditionEvents.OrderBy(x => x.value).ToArray();
        for (int i = 0; i < onAdditionEvents.Length; i++)
            onAdditionEvents[i].ProcessAddition(prev, current);

        int max = Mathf.Max(limits.x, limits.y);
        if ((prev < max) && (current >= max))
            onTopLimitReached?.Invoke();
    }

    void ManageIntSubtraction(int prev, int current)
    {
        onReductionEvents = onReductionEvents.OrderBy(x => -x.value).ToArray();
        for (int i = 0; i < onReductionEvents.Length; i++)
            onReductionEvents[i].ProcessSubtraction(prev, current);

        int min = Mathf.Min(limits.x, limits.y);
        if ((prev > min) && (current <= min))
            onBottomLimitReached?.Invoke();
    }

    void ManageIntChangeGeneric(int prev, int current)
    {
        intChanged?.Invoke(current);
        for (int i = 0; i < onNumberEvents.Length; i++)
        {
            bool prevIn = onNumberEvents[i].IsInRange(prev);

            if (onNumberEvents[i].IsInRange(current))
            {
                if (!prevIn) onNumberEvents[i].Start();
            }
            else if (prevIn) onNumberEvents[i].End();
        }
    }

    void ForceManageIntChangeGeneric(int current, bool isIn = true)
    {
        if (isIn) intChanged?.Invoke(current);
        for (int i = 0; i < onNumberEvents.Length; i++)
            if (onNumberEvents[i].IsInRange(current))
            {
                if (isIn) onNumberEvents[i].Start();
                else onNumberEvents[i].End();
            }
    }

    int AddWithCountMode(int current, int add, Vector2Int limits, CountMode mode)
    {
        switch (mode)
        {
            case CountMode.Clamped:
                return Mathf.Clamp(current + add, limits.x, limits.y);
            case CountMode.Repeat:
                return limits.x +
                    (int)Mathf.Repeat(current - limits.x + add, limits.y - limits.x + 1);
            case CountMode.PingPong:
                pingPongValue = (int)Mathf.Repeat(pingPongValue + add - limits.x, 2 * (limits.y - limits.x));
                return limits.x + (int)Mathf.PingPong(pingPongValue, limits.y - limits.x);
            default:
                return current + add;
        }
    }

    enum CountMode { Infinite, Clamped, Repeat, PingPong }

    [Serializable]
    struct OnVariationEvent
    {
        public int value;
        bool useValueMultipliers;
        [SerializeField]
        [FoldoutGroup("@FoldoutName()")]
        DXEvent actions;

        public OnVariationEvent(int value, bool useValueMultipliers, DXEvent actions)
        {
            this.value = value;
            this.useValueMultipliers = useValueMultipliers;
            this.actions = actions;
        }

        public int Process(int prev, int current)
        {
            int dif = current - prev;
            if (dif > 0) ProcessAddition(prev, current);
            else if (dif < 0) ProcessSubtraction(prev, current);

            return dif;
        }

        public void ProcessAddition(int prev, int current)
        {
            if (!useValueMultipliers)
            {
                if (((prev < value) || (prev > current)) && (current >= value))
                    actions?.Invoke();
            }
            else
            {
                if ((current / value) != (prev / value))
                    actions?.Invoke();
            }
        }

        public void ProcessSubtraction(int prev, int current)
        {
            if (!useValueMultipliers)
            {
                if (((prev > value) || (prev < current)) && (current <= value))
                    actions?.Invoke();
            }
            else
            {
                if ((current / value) != (prev / value))
                    actions?.Invoke();
            }
        }

        public string FoldoutName()
        {
            return actions.IsNull() ? "Events" : "Events ⚠";
        }
    }

    [Serializable]
    struct OnNumberEvent
    {
        [SerializeField]
        int value;
        [SerializeField]
        bool useValueMultipliers;
        [SerializeField]
        bool isRange;
        [ShowIf("isRange")]
        [SerializeField]
        int maxValue;
        [SerializeField]
        GameObject[] objects;
        [SerializeField]
        [FoldoutGroup("@FoldoutName()")]
        DXEvent gotIn;
        [FoldoutGroup("@FoldoutName()")]
        [SerializeField]
        DXEvent gotOut;

        public OnNumberEvent(int value, bool useValueMultipliers, bool isRange, int maxValue)
        {
            this.value = value;
            this.useValueMultipliers = useValueMultipliers;
            this.isRange = isRange;
            this.maxValue = maxValue;
            objects = null;
            gotIn = null;
            gotOut = null;
        }

        public bool IsInRange(int n)
        {
            int i = useValueMultipliers ? (n % value) + value : n;

            if (isRange) return (i >= value) && (i <= maxValue);
            else return (i == value);
        }

        public void Start()
        {
            foreach (GameObject g in objects)
                g.SetActive(true);
            gotIn?.Invoke();
        }

        public void End()
        {
            foreach (GameObject g in objects)
                g.SetActive(false);
            gotOut?.Invoke();
        }

        public string FoldoutName()
        {
            return (gotIn.IsNull() && gotOut.IsNull()) ? "Events" : "Events ⚠";
        }
    }

    [Serializable]
    struct GenericEvents
    {
        public DXEvent intAdded;
        public DXEvent intSubtracted;

        public GenericEvents(DXEvent intAdded, DXEvent intSubtracted)
        {
            this.intAdded = intAdded;
            this.intSubtracted = intSubtracted;
        }
    }
}
