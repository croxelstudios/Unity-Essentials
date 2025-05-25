using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class LookForTransform : MonoBehaviour
{
    [SerializeField]
    [TagSelector]
    string[] tags = null;
    [SerializeField]
    CustomTagItems customTags = new CustomTagItems();
    [SerializeField]
    Transform[] exceptions = null;
    [SerializeField]
    float radius = Mathf.Infinity;
    [SerializeField]
    ChoosingMode choosingMode = ChoosingMode.Nearest;
    [SerializeField]
    bool searchOnEnable = true;
    [SerializeField]
    DXTransformEvent transformFound = null;

    enum ChoosingMode { Nearest, Random, FirstFound }

    void OnEnable()
    {
        if (searchOnEnable) SearchTags();
    }

    public void SearchTags()
    {
        Transform[] possible = FindAllTransforms();

        Transform[] possibleInRadius = FilterTrByRadius(possible, radius);
        if (possibleInRadius.Length > 0)
            possible = possibleInRadius;

        if (possible.Length > 0)
            switch (choosingMode)
            {
                case ChoosingMode.Nearest:
                    float distance = Mathf.Infinity;
                    Transform nearest = null;
                    foreach (Transform tr in possible)
                    {
                        if ((tr != transform) && !exceptions.Any(x => x == tr))
                        {
                            float dist = Vector3.Distance(transform.position, tr.position);
                            if (dist < distance)
                            {
                                nearest = tr;
                                distance = dist;
                            }
                        }
                    }
                    transformFound?.Invoke(nearest);
                    break;
                case ChoosingMode.Random:
                    transformFound?.Invoke(possible[Random.Range(0, possible.Length)].transform);
                    break;
                default:
                    transformFound?.Invoke(possible[0]);
                    break;
            }
    }

    Transform[] FindAllTransforms()
    {
        return CustomTag.FindTransforms(customTags, tags);
        //return FindWithTag.Transforms(tags);
    }

    Transform[] FilterTrByRadius(Transform[] trs, float radius)
    {
        List<Transform> list = new List<Transform>();
        foreach (Transform tr in trs)
        {
            if (Vector3.Distance(tr.position, transform.position) < radius)
                list.Add(tr);
        }
        return list.ToArray();
    }
}
