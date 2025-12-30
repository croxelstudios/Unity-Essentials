#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FindMissingScripts
{
    [MenuItem("Tools/Find Missing Scripts in Project")]
    public static void FindInProject()
    {
        int goCount = 0, missingCount = 0;
        // Escenas abiertas
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(i);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                FindInGameObject(root, scene.path, ref goCount, ref missingCount);
            }
        }
        // Prefabs/assets
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                FindInGameObject(prefab, path, ref goCount, ref missingCount);
            }
        }

        Debug.Log(string.Format("Done. GameObjects scanned: {0}. Missing components found: {1}.", goCount, missingCount));
    }

    static void FindInGameObject(GameObject g, string location, ref int goCount, ref int missingCount)
    {
        goCount++;
        Component[] components = g.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                missingCount++;
                string path = GetGameObjectPath(g);
                Debug.LogWarning(string.Format("Missing script in {0} (at {1}) - GameObject path: {2}", location, AssetDatabase.GetAssetPath(g), path));
            }
        }
        foreach (Transform t in g.transform) FindInGameObject(t.gameObject, location, ref goCount, ref missingCount);
    }

    static string GetGameObjectPath(GameObject go)
    {
        string s = go.name;
        Transform t = go.transform;
        while (t.parent != null)
        {
            t = t.parent;
            s = t.name + "/" + s;
        }
        return s;
    }
}
#endif
