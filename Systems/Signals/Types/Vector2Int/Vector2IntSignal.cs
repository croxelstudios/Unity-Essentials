using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/Vector2IntSignal")] //Change type here
public class Vector2IntSignal : BaseSignal<Vector2Int, Vector2IntSignalListener> //Change type here
{
    [Command("set-vector2int")]
    public static void SetVector2Int(Vector2IntSignal signal, Vector2Int value) //Change type here
    {
        Set(signal, value);
    }

    protected override Vector2Int Calculate() //Change type here
    {
        return currentValue;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Vector2IntSignal))] //Change type here
public class Vector2IntSignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif
