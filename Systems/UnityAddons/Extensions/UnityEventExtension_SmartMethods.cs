using System.Collections.Generic;
using UnityEngine.Events;
using System;

public static class UnityEventExtension_SmartMethods
{
    public static UnityEvent CreateAddListener(this UnityEvent uEvent, UnityAction call)
    {
        if (uEvent == null)
            uEvent = new UnityEvent();
        uEvent.AddListener(call);
        return uEvent;
    }

    public static UnityEvent<T> CreateAddListener<T>(this UnityEvent<T> uEvent, UnityAction<T> call)
    {
        if (uEvent == null)
            uEvent = new UnityEvent<T>();
        uEvent.AddListener(call);
        return uEvent;
    }

    public static void SmartRemoveListener(this UnityEvent uEvent, UnityAction call)
    {
        if (uEvent != null)
            uEvent.RemoveListener(call);
    }

    public static void SmartRemoveListener<T>(this UnityEvent<T> uEvent, UnityAction<T> call)
    {
        if (uEvent != null)
            uEvent.RemoveListener(call);
    }

    static Dictionary<Type, Dictionary<UnityAction, object>> auxActions;

    public static UnityEvent<T> CreateAddListener<T>(this UnityEvent<T> uEvent, UnityAction call)
    {
        if (uEvent == null)
            uEvent = new UnityEvent<T>();
        UnityAction<T> adaptedAction = _ => call();
        auxActions = auxActions.CreateAdd(typeof(T), call, adaptedAction);
        uEvent.AddListener(adaptedAction);
        return uEvent;
    }

    public static void SmartRemoveListener<T>(this UnityEvent<T> uEvent, UnityAction call)
    {
        if ((uEvent != null) &&
            auxActions.SmartGetValue(typeof(T), call, out object action) &&
            (action is UnityAction<T> act))
            uEvent.RemoveListener(act);
    }
}
