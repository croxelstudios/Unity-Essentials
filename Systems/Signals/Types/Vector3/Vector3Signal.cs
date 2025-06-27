using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/Vector3Signal")] //Change type here
public class Vector3Signal : BaseSignal<Vector3> //Change type here
{
    [SerializeField]
    bool callWithSameValue = false;

    [Command("set-vector3")]
    public static void SetVector3(Vector3Signal signal, Vector3 value) //Change type here
    {
        Set(signal, value);
    }

    protected override Vector3 Calculate() //Change type here
    {
        return currentValue;
    }

    protected override bool IsDifferentFromCurrent(Vector3 value)
    {
        return base.IsDifferentFromCurrent(value) || callWithSameValue;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Vector3Signal))] //Change type here
public class Vector3SignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif
