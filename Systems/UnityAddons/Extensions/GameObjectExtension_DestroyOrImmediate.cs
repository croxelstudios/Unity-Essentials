using UnityEngine;

public static class GameObjectExtension_DestroyOrImmediate
{
    public static void DestroyOrImmediate(this GameObject obj)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            Object.DestroyImmediate(obj);
        else
#endif
            Object.Destroy(obj);
    }
}
