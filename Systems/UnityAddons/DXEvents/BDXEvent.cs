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
        unityEvent.AddListener(call);
    }

    public virtual void RemoveListener(UnityAction<T> call)
    {
        unityEvent.RemoveListener(call);
    }
}
