using UnityEngine;
using System.Collections;
using Lowscope.Saving;

public class SaveSystem_Saver : MonoBehaviour
{
    //Saves all the gathered data on the session to the save file
    public void SaveToFile()
    {
        SaveMaster.WriteActiveSaveToDisk();
    }

    public void DeleteSave()
    {
        SaveMaster.ClearListeners(false);
        SaveMaster.DeleteSave();
    }

    public void LoadNewSave()
    {
        SaveMaster.SetSlotAndCopyActiveSave(0);
    }
}
