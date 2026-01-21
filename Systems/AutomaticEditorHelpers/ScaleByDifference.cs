using UnityEngine;

[ExecuteAlways]
public class ScaleByDifference : MonoBehaviour
{
    [SerializeField]
    Transform other = null;
    [SerializeField]
    float difference = 0f;
    [SerializeField]
    Vector3 byAxis = Vector3.one;
    [SerializeField]
    AxisBooleans useAxis = new AxisBooleans(true, true, true);
    [SerializeField]
    bool runtime = false;

    void Update()
    {
        if (runtime
#if UNITY_EDITOR
            || (!Application.isPlaying)
#endif
            )
        {
            Vector3 target = ((other != null) ? other.lossyScale : Vector3.one)
                - (byAxis * difference);
            Vector3 scl = target.InverseScale(transform.lossyScale);
            Vector3 res = transform.localScale;
            res.Scale(scl);
            res.x = useAxis.x ? Mathf.Max(res.x, 0.0001f) : transform.localScale.x;
            res.y = useAxis.y ? Mathf.Max(res.y, 0.0001f) : transform.localScale.y;
            res.z = useAxis.z ? Mathf.Max(res.z, 0.0001f) : transform.localScale.z;
            if (scl.sqrMagnitude.IsBetween(float.NegativeInfinity, float.PositiveInfinity))
                transform.localScale = res;
        }
    }
}
