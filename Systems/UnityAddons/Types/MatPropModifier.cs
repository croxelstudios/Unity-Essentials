using System;
using System.Collections.Generic;
using UnityEngine;

public class MatPropModifier
{
    static Dictionary<SharedMatProp, List<MatPropModifier>> dict;
    static event Action update;
    SharedMatProp matProp;
    int priority;
    Action onUpdate;

    public MatPropModifier(SharedMatProp matProp, int priority, Action onUpdate = null)
    {
        this.matProp = matProp;
        this.priority = priority;
        this.onUpdate = onUpdate;
        Register();
    }

    public MatPropModifier(Material material, string property, int priority, Action onUpdate = null)
    {
        matProp = new SharedMatProp(material, property);
        this.priority = priority;
        this.onUpdate = onUpdate;
        Register();
    }

    void Register()
    {
        dict = dict.CreateAdd(matProp, this);
        if (IsOnlyMaxPriority())
            update?.Invoke();
        if (onUpdate != null)
            update += onUpdate;
    }

    public bool CanAct()
    {
        if (dict.TryGetValue(matProp, out List<MatPropModifier> list))
            foreach (MatPropModifier m in list)
            {
                if (m.priority > priority)
                    return false;
            }
        return true;
    }

    public void Dispose()
    {
        bool wasMax = IsOnlyMaxPriority();
        dict.Remove(matProp);
        if (onUpdate != null)
            update -= onUpdate;
        if (wasMax)
            update?.Invoke();
    }

    bool IsOnlyMaxPriority()
    {
        if (dict.TryGetValue(matProp, out List<MatPropModifier> list))
            foreach (MatPropModifier m in list)
            {
                if ((m != this) && (m.priority >= priority))
                    return false;
            }
        return true;
    }
}
