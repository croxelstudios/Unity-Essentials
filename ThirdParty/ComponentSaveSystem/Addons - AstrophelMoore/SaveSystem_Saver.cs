using Lowscope.Saving;
using UnityEngine;

public class SaveSystem_Saver : MonoBehaviour
{
    [SerializeField]
    DXEvent onSaveFilePresent = null;
    [SerializeField]
    DXEvent onSaveFileAbsent = null;

    //Saves all the gathered data on the session to the save file
    public void SaveToFile()
    {
        SaveMaster.WriteActiveSaveToDisk();
        CheckSlots();
    }

    public void DeleteSave()
    {
        BSaver.ResetSavers();
        SaveToFile();
    }

    public void LoadNewSave()
    {
        SaveMaster.SetSlotAndCopyActiveSave(0);
    }

    void OnEnable()
    {
        CheckSlots();
    }

    public void CheckSlots()
    {
        int[] slots = SaveMaster.GetUsedSlots();
        if (slots.IsNullOrEmpty())
            onSaveFileAbsent?.Invoke();
        else
            onSaveFilePresent?.Invoke();
    }
}
