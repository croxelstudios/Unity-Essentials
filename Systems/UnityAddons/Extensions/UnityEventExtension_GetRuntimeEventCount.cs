using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public static class UnityEventExtension_GetRuntimeEventCount
{
    public static int GetRuntimeEventCount(this UnityEventBase unityEvent)
    {
        return ReflectionTools.GetValue<int>(unityEvent, "m_Calls.m_RuntimeCalls.Count");
    }

    public static int GetTotalEventCount(this UnityEventBase unityEvent)
    {
        return ReflectionTools.InvokePrivateMethod<System.Collections.ICollection>(unityEvent, "PrepareInvoke").Count;
        //return unityEvent.GetRuntimeEventCount() + unityEvent.GetPersistentEventCount();
    }
}
