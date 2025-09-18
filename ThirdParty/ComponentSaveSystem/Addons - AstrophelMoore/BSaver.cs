using UnityEngine;
using Lowscope.Saving;
using System.Collections.Generic;

public class BSaver : MonoBehaviour, ISaveable
{
    static List<BSaver> savers;

    public static void ResetSavers()
    {
        foreach (BSaver saver in savers)
            saver.ResetSaver();
    }

    protected virtual void OnEnable()
    {
        savers = savers.CreateAdd(this);
    }

    protected virtual void OnDisable()
    {
        savers.Remove(this);
    }

    public virtual void OnLoad(string data)
    {
        if (this.IsActiveAndEnabled())
        {
            Load(data);
        }
    }

    public virtual void Load(string data)
    {

    }

    public string OnSave()
    {
        if (this.IsActiveAndEnabled())
        {
            return Save();
        }
        else return "";
    }

    public virtual string Save()
    {
        return "";
    }

    public bool OnSaveCondition()
    {
        return ShouldISave();
    }

    public virtual bool ShouldISave()
    {
        return true;
    }

    public virtual void ResetSaver()
    {

    }
}
