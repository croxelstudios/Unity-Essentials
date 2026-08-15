using UnityEngine;
using UnityEngine.Events;

public class BDXEvent<T> : Wrapper where T : UnityEventBase
{
    [SerializeField]
    [EasyEvent]
    protected T unityEvent = null;

    protected override bool IsNull()
    {
        return (unityEvent == null) || (unityEvent.GetTotalEventCount() <= 0);
    }

    public void Clear()
    {
        if (unityEvent != null)
            unityEvent.RemoveAllListeners();
    }
}

public class DXTypedEvent<T> : BDXEvent<UnityEvent<T>>
{
    public virtual void Invoke(T arg0)
    {
        unityEvent?.Invoke(arg0);
    }

    public virtual void AddListener(UnityAction call)
    {
        unityEvent = unityEvent.CreateAddListener(call);
    }

    public virtual void RemoveListener(UnityAction call)
    {
        unityEvent.SmartRemoveListener(call);
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
        if (dxEvent is null)
            dxEvent = new DXEvent();
        return dxEvent;
    }

    public static T CreateIfNull<T, J>(this T dxEvent)
        where T : DXTypedEvent<J>, new()
    {
        if (dxEvent is null)
            dxEvent = new T();
        return dxEvent;
    }

    public static DXEvent CreateAddListener(this DXEvent dxEvent, UnityAction call)
    {
        dxEvent = dxEvent.CreateIfNull();
        dxEvent.AddListener(call);
        return dxEvent;
    }

    public static T CreateAddListener<T, J>(this T dxEvent, UnityAction<J> call)
        where T : DXTypedEvent<J>, new()
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
