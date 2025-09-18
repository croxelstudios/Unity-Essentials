using UnityEngine;
using UnityEngine.Events;

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
}
