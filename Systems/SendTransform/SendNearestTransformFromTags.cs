using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class SendNearestTransformFromTags : MonoBehaviour
{
    [SerializeField]
    [TagSelector]
    string[] tags = null;
    [SerializeField]
    DXTransformEvent transformFound = null;

    public void SearchTags()
    {
        GameObject[] possibleGO = FindByTags(tags);
        float distance = Mathf.Infinity;
        Transform nearest = null;
        foreach (GameObject obj in possibleGO)
        {
            if (obj != gameObject)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < distance)
                {
                    nearest = obj.transform;
                    distance = dist;
                }
            }
        }
        transformFound?.Invoke(nearest);
    }

    GameObject[] FindByTags(string[] tags)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (string tag in tags) list.AddRange(GameObject.FindGameObjectsWithTag(tag));
        return list.ToArray();
    }
}
