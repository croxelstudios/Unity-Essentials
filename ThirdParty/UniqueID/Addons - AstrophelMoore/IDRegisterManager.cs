using CleverCrow.Fluid.UniqueIds;
using UnityEngine;

[RequireComponent(typeof(IUniqueId))]
public class IDRegisterManager : MonoBehaviour
{
    [SerializeField]
    StringList idList = null;
    [SerializeField]
    bool launchOnEnable = true;
    [SerializeField]
    DXEvent idEntersRegister = null;
    [SerializeField]
    DXEvent idExitsRegister = null;

    public StringList IdList
    {
        get => idList;
        set { idList = value; CheckIDs(); }
    }
    IUniqueId idHolder;

    void Awake()
    {
        idHolder = GetComponent<IUniqueId>();
    }

    void OnEnable()
    {
        if (launchOnEnable)
            CheckIDs();
        idList.valueAdded = idList.valueAdded.CreateAddListener<DXStringEvent, string>(CheckAdded);
        idList.valueRemoved = idList.valueRemoved.CreateAddListener<DXStringEvent, string>(CheckRemoved);
    }

    void OnDisable()
    {
        idList.valueAdded.SmartRemoveListener<DXStringEvent, string>(CheckAdded);
        idList.valueRemoved.SmartRemoveListener<DXStringEvent, string>(CheckRemoved);
    }

    void CheckIDs()
    {
        if (idList.HasValue(idHolder.Id))
            idEntersRegister?.Invoke();
        else idExitsRegister?.Invoke();
    }

    void CheckAdded(string value)
    {
        if (value == idHolder.Id)
            idEntersRegister?.Invoke();
    }

    void CheckRemoved(string value)
    {
        if (value == idHolder.Id)
            idExitsRegister?.Invoke();
    }

    public void RegisterId()
    {
        idList.AddValue(idHolder.Id);
    }

    public void RemoveId()
    {
        idList.RemoveValue(idHolder.Id);
    }
}
