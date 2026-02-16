using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class DXTexture2DEvent : DXTypedEvent<Texture2D>
{ }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DXTexture2DEvent))]
public class DXTexture2DEventDrawer : DXDrawerBase
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}
#endif