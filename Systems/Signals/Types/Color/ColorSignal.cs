using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/ColorSignal")] //Change type here
public class ColorSignal : BaseSignal<Color, ColorSignalListener> //Change type here
{
    [Command("set-color")]
    public static void SetColor(ColorSignal signal, Color value) //Change type here
    {
        Set(signal, value);
    }

    protected override Color Calculate() //Change type here
    {
        return currentValue;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ColorSignal))] //Change type here
public class ColorSignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif
