using UnityEngine;
using Sirenix.OdinInspector;
using QFSW.QC;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/SignalTypes/GameObjectSignal")] //Change type here
public class GameObjectSignal : ValueSignal<GameObject> //Change type here
{
    [Command("set-gameobject")]
    public static void SetGameObject(GameObjectSignal signal, GameObject value) //Change type here
    {
        Set(signal, value);
    }

    protected override GameObject Calculate() //Change type here
    {
        return currentValue;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(GameObjectSignal))] //Change type here
public class GameObjectSignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif
