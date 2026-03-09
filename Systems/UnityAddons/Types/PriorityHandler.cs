using System;
using System.Collections.Generic;

public class PriorityHandler<K> where K : IEquatable<K>
{
    static Dictionary<K, List<PriorityHandler<K>>> dict;
    static event Action update;
    K key;
    int priority;
    Action onUpdate;

    public PriorityHandler()
    {
    }

    public PriorityHandler(K key, int priority, Action onUpdate = null)
    {
        Register(key, priority, onUpdate);
    }

    public void Register(K key, int priority, Action onUpdate = null)
    {
        this.key = key;
        this.priority = priority;
        this.onUpdate = onUpdate;
        Register();
    }

    void Register()
    {
        if (!dict.NotNullContainsKey(key))
        {
            dict = dict.CreateAdd(key, this);
            if (IsOnlyMaxPriority())
                update?.Invoke();
            if (onUpdate != null)
                update += onUpdate;
        }
    }

    public bool CanAct()
    {
        if (dict.TryGetValue(key, out List<PriorityHandler<K>> list))
            foreach (PriorityHandler<K> m in list)
            {
                if (m.priority > priority)
                    return false;
            }
        return true;
    }

    public void Dispose()
    {
        bool wasMax = IsOnlyMaxPriority();
        dict.SmartRemoveClear(key, this);
        if (onUpdate != null)
            update -= onUpdate;
        if (wasMax)
            update?.Invoke();
    }

    bool IsOnlyMaxPriority()
    {
        if (dict.TryGetValue(key, out List<PriorityHandler<K>> list))
            foreach (PriorityHandler<K> m in list)
            {
                if ((m != this) && (m.priority >= priority))
                    return false;
            }
        return true;
    }
}

public static class PriorityHandler_Extension
{
    public static PriorityHandler<K> CreateRegister<K>(this PriorityHandler<K> modifier,
        K key, int priority, Action onUpdate = null) where K : IEquatable<K>
    {
        if (modifier == null)
            modifier = new PriorityHandler<K>();
        modifier.Register(key, priority, onUpdate);
        return modifier;
    }

    //WARNING: Doesn't work?
    //public static void TryDispose<K>(this PriorityHandler<K> modifier) where K : IEquatable<K>
    //{
    //    if (modifier != null)
    //        modifier.Dispose();
    //}
}
