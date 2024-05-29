using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class SendRandomTransformFromTags : MonoBehaviour
{
    [SerializeField]
    [TagSelector]
    string[] tags = null;
    [SerializeField]
    float radius = 4;
    [SerializeField]
    DXTransformEvent transformFound = null;

    public void SearchTags()
    {
        GameObject[] possibleGO = FindByTags(tags);
        GameObject[] possibleGOInRadius = FilterTrByRadius(possibleGO, radius);
        if (possibleGOInRadius.Length > 0)
            transformFound?.Invoke(possibleGOInRadius[Random.Range(0, possibleGOInRadius.Length)].transform);
        else
            transformFound?.Invoke(possibleGO[Random.Range(0, possibleGO.Length)].transform);
    }

    GameObject[] FindByTags(string[] tags)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (string tag in tags) list.AddRange(GameObject.FindGameObjectsWithTag(tag));
        return list.ToArray();
    }

    GameObject[] FilterTrByRadius(GameObject[] objs, float radius)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (GameObject obj in objs)
        {
            if (Vector3.Distance(obj.transform.position, transform.position) < radius)
                list.Add(obj);
        }
        return list.ToArray();
    }
}
