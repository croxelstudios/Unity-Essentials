using UnityEngine;

[ExecuteInEditMode]
public class RenderersSetColor_FromDistance : RenderersSetColor
{
    [SerializeField]
    [TagSelector]
    string targetTag = "MainCamera";
    [SerializeField]
    Vector2 range = Vector2.up;
    [SerializeField]
    Color nearColor = Color.clear;
    [SerializeField]
    Color farColor = Color.white;
    [SerializeField]
    bool updateTransforms = false;

    Transform target;
    protected override void Init()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            float distance = Mathf.Infinity;
            target = GetClosest(targetTag, ref distance);
            base.Init();
        }
    }

    protected override void UpdateBehaviour()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            float distance = Mathf.Infinity;
            if (updateTransforms) target = GetClosest(targetTag, ref distance);
            else distance = Vector3.Distance(transform.position, target.position);
            color = Color.Lerp(nearColor, farColor, Mathf.InverseLerp(range.x, range.y, distance));
            base.UpdateBehaviour();
        }
    }

    public void UpdateTaggedObjects()
    {
        float distance = Mathf.Infinity;
        target = GetClosest(targetTag, ref distance);
    }

    public void ChangeTag(string tag)
    {
        targetTag = tag;
        UpdateTaggedObjects();
    }

    Transform GetClosest(string tag, ref float distance)
    {
        GameObject[] targetsGO = GameObject.FindGameObjectsWithTag(tag);

        Transform tr = null;
        distance = Mathf.Infinity;
        foreach (GameObject target in targetsGO)
        {
            Transform tran = target.transform;
            float newDistance = Vector3.Distance(transform.position, tran.position);
            if (newDistance < distance)
            {
                distance = newDistance;
                tr = tran;
            }
        }
        return tr;
    }
}
