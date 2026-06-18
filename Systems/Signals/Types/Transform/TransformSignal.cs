using UnityEngine;
using QFSW.QC;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/TransformSignal")] //Change type here
public class TransformSignal : ValueSignal<Transform> //Change type here
{
    [Command("set-gameobject")]
    public static void SetGameObject(TransformSignal signal, Transform value) //Change type here
    {
        Set(signal, value);
    }

    protected override Transform Calculate() //Change type here
    {
        return currentValue;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TransformSignal))] //Change type here
public class TransformSignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif
