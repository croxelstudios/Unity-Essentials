using UnityEngine;
using UnityEngine.Events;

public static class UnityEventExtension_GetRuntimeEventCount
{
    public static int GetRuntimeEventCount(this UnityEventBase unityEvent)
    {
        return ReflectionTools.GetValue<int>(unityEvent, "m_Calls.m_RuntimeCalls.Count");
    }

    public static int GetTotalEventCount(this UnityEventBase unityEvent)
    {
        return ReflectionTools.InvokePrivateMethod<int>(unityEvent, "GetCallsCount");
        //return unityEvent.GetRuntimeEventCount() + unityEvent.GetPersistentEventCount();
    }
}
