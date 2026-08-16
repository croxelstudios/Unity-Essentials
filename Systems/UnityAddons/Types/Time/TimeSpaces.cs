using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "TimeSpaces", menuName = "Croxel Scriptables/TimeSpaces")]
public class TimeSpaces : ScriptableObject
{
    public static TimeSpaces instance;
    public string[] timeSpaces = null;

    void OnEnable()
    {
        if (instance == null) instance = this;
    }

    void Reset()
    {
        timeSpaces = new string[] { "Default" };
    }

    public static string[] Names()
    {
        if (instance == null)
            return new string[0];
        return instance.timeSpaces;
    }
}

#if UNITY_EDITOR
[InitializeOnLoad]
public static class TimeSpaces_Initializer
{
    static TimeSpaces_Initializer()
    {
        string[] guids = AssetDatabase.FindAssets("t:TimeSpaces");
        if (!guids.IsNullOrEmpty())
            TimeSpaces.instance = AssetDatabase.LoadAssetAtPath<TimeSpaces>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
}
#endif
