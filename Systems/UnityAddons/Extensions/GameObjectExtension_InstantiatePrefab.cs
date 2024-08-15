using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class GameObjectExtension_InstantiatePrefab
{
    public static GameObject InstantiatePrefab(this GameObject obj, Vector3 position,
        Quaternion rotation, Transform parent)
    {
        GameObject newObj;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            newObj = (GameObject)PrefabUtility.InstantiatePrefab(obj, parent);
            newObj.transform.position = position;
            newObj.transform.rotation = rotation;
        }
        else
#endif
            newObj = Object.Instantiate(obj, position, rotation, parent);
        newObj.name = obj.name;
        return newObj;
    }

    public static GameObject InstantiatePrefab(this GameObject obj, Transform parent)
    {
        GameObject newObj;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            newObj = (GameObject)PrefabUtility.InstantiatePrefab(obj, parent);
        }
        else
#endif
            newObj = Object.Instantiate(obj, parent);
        newObj.name = obj.name;
        return newObj;
    }
}
