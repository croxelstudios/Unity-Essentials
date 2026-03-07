using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IEventActionAttribute
{
    public bool InterpretInEventsDrawer(Rect argRect,
        SerializedProperty argument, SerializedProperty listenerTarget);
}
