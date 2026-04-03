using CleverCrow.Fluid.UniqueIds;
using UnityEngine;

[DefaultExecutionOrder(-9000)]
[RequireComponent(typeof(IUniqueId))]
public class IDSignalManager : BBaseSignalListener<string>
{
    [SerializeField]
    StringSignal signal = null;
    [SerializeField]
    DXEvent isThisID = null;
    [SerializeField]
    DXEvent isNotThisID = null;

    public override void UpdateSignals()
    {
        base.UpdateSignals();
        signals = new StringSignal[] { signal };
    }

    public override void LaunchActions()
    {
        LaunchActions(0, signal.currentValue);
    }

    public override void LaunchActions(int index, string value)
    {
        if (value != idHolder.Id)
            isNotThisID?.Invoke();
        else
            isThisID?.Invoke();
    }

#if UNITY_EDITOR
    [ContextMenu("Create new StringSignal")] //Change type here
    void CreateSignal()
    {
        ScriptableObjectUtils.CreateScriptableObjectAsset<StringSignal>("StringSignal", "New StringSignal"); //Change type here (3)
    }
#endif

    IUniqueId idHolder = null;

    void Awake()
    {
        idHolder = GetComponent<IUniqueId>();
    }

    public void SetID()
    {
        signal.CallSignal(idHolder.Id);
    }

    public void ResetID()
    {
        signal.CallSignal("");
    }
}
