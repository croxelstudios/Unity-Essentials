using System.Collections.Generic;
using UnityEngine;

public class NearestCustomTag : MonoBehaviour
{
    [SerializeField]
    CustomTagItems customTags = new CustomTagItems();
    [SerializeField]
    DXTransformEvent nearest = null;
    [SerializeField]
    TimeModeOrOnEnable timeMode = TimeModeOrOnEnable.Update;
    [SerializeField]
    bool considerOwnParents = false;

    Transform last;

    void OnEnable()
    {
        if (timeMode.OnEnable()) SendNearest();
    }

    void Update()
    {
        if (timeMode.IsSmooth()) SendNearest();
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) SendNearest();
    }

    Transform CheckNearest()
    {
        Transform result = null;
        float sqrDistance = Mathf.Infinity;
        
        if (!CustomTag.activeTagged.ContainsKey(customTags.tagList)) return null; //
        for (int i = 0; i < customTags.customTags.Length; i++)
        {
            if (!CustomTag.activeTagged[customTags.tagList].ContainsKey(customTags.customTags[i])) continue; //
            List<CustomTag> list = CustomTag.activeTagged[customTags.tagList][customTags.customTags[i]];
            for (int j = 0; j < list.Count; j++)
            {
                if ((!considerOwnParents) && transform.IsChildOf(list[j].transform)) continue; //
                GameObject[] objs = list[j].includedGameObjects;
                for (int k = 0; k < objs.Length; k++)
                {
                    Vector3 dif = transform.position - objs[k].transform.position;
                    float sqrDist = dif.sqrMagnitude;
                    if (sqrDist < sqrDistance)
                    {
                        sqrDistance = sqrDist;
                        result = objs[k].transform;
                    }
                }
            }
        }
        return result;
    }

    void SendNearest()
    {
        Transform result = CheckNearest();
        if (result != last)
        {
            last = result;
            nearest?.Invoke(result);
        }
    }
}
