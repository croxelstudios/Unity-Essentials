using UnityEngine;

public class BControllersData : ScriptableObject
{
    public virtual void AwakeData()
    {

    }

    public virtual bool UpdateData()
    {
        return false;
    }

    public virtual int IdentifyController()
    {
        return -2;
    }

    public virtual bool IsGamepadConnected()
    {
        return false;
    }
}
