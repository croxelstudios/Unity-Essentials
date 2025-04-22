using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlagCounter : MonoBehaviour
{
    [SerializeField]
    string flagNameSubstringToCount = "";
    [SerializeField]
    bool checkFlagIsTrue = true;
    [SerializeField]
    Flag[] exceptions = null;

    [SerializeField]
    DXIntEvent flagsCounted = null;

    List<Flag> activeFlags = null;
    int count;

    void OnEnable()
    {
        if (activeFlags == null)
            activeFlags = new List<Flag>();
        activeFlags.AddRange(
            Array.ConvertAll(BaseSignal.activeSignals[typeof(Flag)].ToArray(), item => (Flag)item));

        for (int i = activeFlags.Count - 1; i >= 0; i--)
            if (exceptions.Contains(activeFlags[i]) || !activeFlags[i].name.Contains(flagNameSubstringToCount))
                activeFlags.RemoveAt(i);

        if (checkFlagIsTrue)
            for (int i = 0; i < activeFlags.Count; i++)
                SubscribeToFlag(activeFlags[i]);

        CountFlags();

        BaseSignal.OnEnableCallback += RegisterFlag;
    }

    void OnDisable()
    {
        if (checkFlagIsTrue)
            for (int i = 0; i < activeFlags.Count; i++)
                UnsubscribeFromFlag(activeFlags[i]);

        activeFlags = null;

        BaseSignal.OnEnableCallback -= RegisterFlag;
    }

    void RegisterFlag(Type ty, BaseSignal sig)
    {
        if (ty == typeof(Flag))
        {
            Flag f = (Flag)sig;
            if (!exceptions.Contains(f) && f.name.Contains(flagNameSubstringToCount))
                activeFlags.Add(f);
            SubscribeToFlag(f);
        }
    }

    void SubscribeToFlag(Flag f)
    {
        f.whenTrue.AddListener(Add);
        f.whenFalse.AddListener(Remove);
    }

    void UnsubscribeFromFlag(Flag f)
    {
        f.whenTrue.RemoveListener(Add);
        f.whenFalse.RemoveListener(Remove);
    }

    void Add()
    {
        count++;
        flagsCounted?.Invoke(count);
    }

    void Remove()
    {
        count--;
        flagsCounted?.Invoke(count);
    }

    public void CountFlags()
    {
        int count = 0;
        /*
        Flag[] flags = Array.ConvertAll(BaseSignal.activeSignals[typeof(Flag)].ToArray(), item => (Flag)item);

        foreach (Flag f in flags)
        {
            if (!exceptions.Contains(f) && f.name.Contains(flagNameSubstringToCount) &&
                (!checkFlagIsTrue || f.currentValue))
                count++;
        }
        */

        for (int i = 0; i < activeFlags.Count; i++)
            if ((!checkFlagIsTrue) || activeFlags[i].currentValue)
                count++;

        this.count = count;

        flagsCounted?.Invoke(count);
    }
}
