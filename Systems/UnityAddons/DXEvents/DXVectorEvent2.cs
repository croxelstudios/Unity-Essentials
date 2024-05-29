using System;
using UnityEditor;

[Serializable]
public class DXVectorEvent2 : DXVectorEvent
{
    public DXVectorEvent2() { types = new EventType[] { EventType.Vector2 }; }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXVectorEvent2))]
public class DXVectorEvent2Drawer : DXVectorEventDrawer
{

}

#endif
