using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/FloatSignal")] //Change type here
public class FloatSignal : BaseSignal<float, FloatSignalListener> //Change type here
{
    [SerializeField]
    bool callWithSameValue = false;

    [Command("set-float")]
    public static void SetFloat(FloatSignal signal, float value) //Change type here
    {
        Set(signal, value);
    }

    protected override float Calculate() //Change type here
    {
        return currentValue;
    }

    protected override bool IsDifferentFromCurrent(float value)
    {
        return base.IsDifferentFromCurrent(value) || callWithSameValue;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FloatSignal))] //Change type here
public class FloatSignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif

#if PLAYMAKER
namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Croxel")]
    [Tooltip("Calls a float signal")]
    public class PMCallFloatSignal : FsmStateAction
    {
        [Tooltip("Float to send")]
        public FsmFloat value;

        public FloatSignal signal;

        // Code that runs on entering the state.
        public override void OnEnter()
        {
            signal.CallSignal(value.Value);
            Finish();
        }
    }
}
#endif