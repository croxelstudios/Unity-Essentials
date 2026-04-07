using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IEventActionAttribute
{
#if UNITY_EDITOR
    public bool InterpretInEventsDrawer(Rect argRect,
        SerializedProperty argument, SerializedProperty listenerTarget);
#endif
}
