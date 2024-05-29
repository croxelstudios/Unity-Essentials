using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/EventNamesData")]
public class EventNamesData : ScriptableObject
{
    public string[] names;

    void OnValidate()
    {
        
    }

    public void SyncNames(ref string[] localNames, bool priorizeLocal = false)
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Event names data scriptable change");
#endif
        if (((names == null) || (names.Length <= 0)) &&
            ((localNames != null) || (localNames.Length > 0)))
            names = localNames;
        else
        {
            if (names.Length < localNames.Length)
            {
                for (int i = names.Length; i < localNames.Length; i++)
                    names = names.Append(localNames[i]).ToArray();
            }
            for (int i = 0; i < localNames.Length; i++)
            {
                if (localNames[i] != names[i])
                {
                    if (priorizeLocal)
                    {
                        if (localNames[i] == "") localNames[i] = names[i];
                        else names[i] = localNames[i];
                    }
                    else
                    {
                        if (names[i] == "") names[i] = localNames[i];
                        else localNames[i] = names[i];
                    }
                }
            }
        }
    }
}

//TO DO: All this is going to probably be removed and changed to work with signals, including things like the PrefabInstancer
public interface IEventNamesDataUpdatable
{
    public void EventNamesDataChanged(EventNamesData namesData);
}
