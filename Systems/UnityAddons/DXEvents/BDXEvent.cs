using UnityEngine;
using UnityEngine.Events;

public class BDXEvent<T> where T : UnityEventBase
{
    [SerializeField]
    protected T unityEvent = null;

    public virtual bool IsNull()
    {
        return unityEvent == null || unityEvent.GetPersistentEventCount() <= 0;
    }
}

public class DXTypedEvent<T> : BDXEvent<UnityEvent<T>>
{
    public virtual void Invoke(T arg0)
    {
        unityEvent?.Invoke(arg0);
    }

    public virtual void AddListener(UnityAction<T> call)
    {
        unityEvent = unityEvent.CreateAddListener(call);
    }

    public virtual void RemoveListener(UnityAction<T> call)
    {
        unityEvent.SmartRemoveListener(call);
    }
}

public static class DXEventExtensions
{
    public static DXEvent CreateIfNull(this DXEvent dxEvent)
    {
        if (dxEvent == null)
            dxEvent = new DXEvent();
        return dxEvent;
    }

    public static T CreateIfNull<T, J>(this T dxEvent)
        where T : DXTypedEvent<J>
    {
        if (dxEvent == null)
            dxEvent = new DXTypedEvent<J>() as T;
        return dxEvent;
    }

    public static DXEvent CreateAddListener(this DXEvent dxEvent, UnityAction call)
    {
        dxEvent = dxEvent.CreateIfNull();
        dxEvent.AddListener(call);
        return dxEvent;
    }

    public static T CreateAddListener<T, J>(this T dxEvent, UnityAction<J> call)
        where T : DXTypedEvent<J>
    {
        dxEvent = dxEvent.CreateIfNull<T, J>();
        dxEvent.AddListener(call);
        return dxEvent;
    }

    public static void SmartRemoveListener(this DXEvent dxEvent, UnityAction call)
    {
        if (dxEvent != null)
            dxEvent.RemoveListener(call);
    }

    public static void SmartRemoveListener<T, J>(this T dxEvent, UnityAction<J> call)
        where T : DXTypedEvent<J>
    {
        if (dxEvent != null)
            dxEvent.RemoveListener(call);
    }
}
