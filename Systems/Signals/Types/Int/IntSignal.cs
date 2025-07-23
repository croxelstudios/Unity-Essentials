using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/IntSignal")] //Change type here
public class IntSignal : ValueSignal<int> //Change type here
{
    protected override void SetValue(int value) //Change type here
    {
        base.SetValue(value);
    }
    //TO DO: Add and Subtract events. Repeat these n times when the value jumps?
    //Should be implemented in function above.

    [Command("set-int")]
    public static void SetInt(IntSignal signal, int value) //Change type here
    {
        Set(signal, value);
    }

    public void Add(int amount, string tag = "")
    {
        CallSignal(currentValue + amount, tag);
    }

    public void Subtract(int amount, string tag = "")
    {
        Add(-amount, tag);
    }

    public void Add(int amount)
    {
        Add(amount, "");
    }

    public void Subtract(int amount)
    {
        Subtract(amount, "");
    }

    public void Add()
    {
        Add(1, "");
    }

    public void Subtract()
    {
        Subtract(1, "");
    }

    protected override int Calculate() //Change type here
    {
        return currentValue;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(IntSignal))] //Change type here
public class IntSignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif

#if PLAYMAKER
namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Croxel")]
    [Tooltip("Calls a int signal")]
    public class PMCallIntSignal : FsmStateAction
    {
        [Tooltip("Int to send")]
        public FsmInt value;

        public IntSignal signal;

        // Code that runs on entering the state.
        public override void OnEnter()
        {
            signal.CallSignal(value.Value);
            Finish();
        }
    }
}
#endif