using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/FloatSignal")] //Change type here
public class FloatSignal : ValueSignal<float> //Change type here
{
    [SerializeField]
    bool callWithSameValue = false;

    [Command("set-float")]
    public static void SetFloat(FloatSignal signal, float value) //Change type here
    {
        Set(signal, value);
    }

    public void Add(float amount, string tag = "")
    {
        CallSignal(currentValue + amount, tag);
    }

    public void Subtract(float amount, string tag = "")
    {
        Add(-amount, tag);
    }

    public void Add(float amount)
    {
        Add(amount, "");
    }

    public void Subtract(float amount)
    {
        Subtract(amount, "");
    }

    public void Add()
    {
        Add(1f, "");
    }

    public void Subtract()
    {
        Subtract(1f, "");
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