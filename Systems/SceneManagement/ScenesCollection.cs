using UnityEngine;

[CreateAssetMenu(fileName = "New Scenes Collection", menuName = "Croxel Scriptables/Collections/Scenes Collection", order = 1)]
public class ScenesCollection : ScriptableObject
{
    public SceneReference[] scenes = null;
}
