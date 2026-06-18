using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class LookForTransform : DXMonoBehaviour
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
    Timing timing = Timing.OnEnable;
    [SerializeField]
    DXTransformEvent transformFound = null;

    Transform[] possible;

    enum Timing { OnEnable, Update, OnCall, OnCallUpdateTransforms }
    enum ChoosingMode { Nearest, Random, FirstFound }

    void OnEnable()
    {
        UpdateTransforms();
        if (timing == Timing.OnEnable) SearchTags();
    }

    void Update()
    {
        if (timing == Timing.Update) SearchTags();
    }

    public void UpdateTransforms()
    {
        possible = FindAllTransforms();
    }

    public void SearchTags()
    {
        if (timing == Timing.OnCallUpdateTransforms)
            UpdateTransforms();

        Transform[] possibleInRadius = FilterTrByRadius(possible, radius);

        if (possibleInRadius.Length > 0)
            switch (choosingMode)
            {
                case ChoosingMode.Nearest:
                    float distance = Mathf.Infinity;
                    Transform nearest = null;
                    foreach (Transform tr in possibleInRadius)
                        if ((tr != transform) && !exceptions.Any(x => x == tr))
                        {
                            float dist = Vector3.Distance(transform.position, tr.position);
                            if (dist < distance)
                            {
                                nearest = tr;
                                distance = dist;
                            }
                        }
                    transformFound?.Invoke(nearest);
                    break;
                case ChoosingMode.Random:
                    transformFound?.Invoke(possibleInRadius[Random.Range(0, possible.Length)].transform);
                    break;
                default:
                    transformFound?.Invoke(possibleInRadius[0]);
                    break;
            }
    }

    Transform[] FindAllTransforms()
    {
        return CustomTag.FindTransforms(customTags, tags);
        //return FindWithTag.Transforms(tags);
    }
    
    List<Transform> aux;

    Transform[] FilterTrByRadius(Transform[] trs, float radius)
    {
        if (trs.IsNullOrEmpty())
            return new Transform[0];

        aux = aux.ClearOrCreate();
        foreach (Transform tr in trs)
        {
            if (Vector3.Distance(tr.position, transform.position) < radius)
                aux.Add(tr);
        }
        return aux.ToArray();
    }
}
