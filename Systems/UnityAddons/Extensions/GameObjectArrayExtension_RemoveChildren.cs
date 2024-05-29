using System.Collections.Generic;
using UnityEngine;

public static class GameObjectArrayExtension_RemoveChildren
{
    public static GameObject[] CleanChildren(this GameObject[] objs)
    {
        List<GameObject> list = new List<GameObject>();
        list.AddRange(objs);
        foreach (GameObject objp in objs)
            foreach (GameObject obj in objs)
                if ((obj != objp) && obj.transform.IsChildOf(objp.transform))
                    list.Remove(obj);
        return list.ToArray();
    }

    public static Transform[] RemoveChildren(this Transform[] objs)
    {
        List<Transform> list = new List<Transform>();
        list.AddRange(objs);
        foreach (Transform objp in objs)
            foreach (Transform obj in objs)
                if ((obj != objp) && obj.IsChildOf(objp))
                    list.Remove(obj);
        return list.ToArray();
    }
}
