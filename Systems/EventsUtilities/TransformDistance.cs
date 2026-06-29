using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public class TransformDistance : MonoBehaviour
{
    [SerializeField]
    ObjectRef<Transform> origin = new ObjectRef<Transform>("Origin", "Player");
    [SerializeField]
    ObjectRef<Transform> target = new ObjectRef<Transform>("Target", "MainCamera");
    [SerializeField]
    bool local = false;
    [SerializeField]
    bool lerpDistance = true;
    [Indent]
    [SerializeField]
    [ShowIf("lerpDistance")]
    [OnValueChanged("RestrictRange")]
    Vector2 distanceRange = new Vector2(0f, 10f);
    [SerializeField]
    float multiplier = 1f;
    [SerializeField]
    bool launchInEditor = false;
    [SerializeField]
    DXFloatEvent distance = new DXFloatEvent();

    float lastValue = -1f;

    void Reset()
    {
        origin = new ObjectRef<Transform>("Origin", transform);
    }

    void Update()
    {
#if UNITY_EDITOR
        if ((!Application.isPlaying) && (!launchInEditor))
            return;
#endif
        Transform targ = (Transform)target;
        Transform orig = (Transform)origin;
        if ((targ != null) && (orig != null))
        {
            origin.SetClosest(targ);
            orig = (Transform)origin;
            target.SetClosest(orig);
            targ = (Transform)target;
            Vector3 other = targ.position;
            if (local)
                other = orig.InverseTransformPoint(other);
            float dist = Vector3.Distance(local ? orig.localPosition : orig.position, other);
            if (lerpDistance)
                dist = Mathf.InverseLerp(distanceRange.x, distanceRange.y, dist);
            dist *= multiplier;
            if (dist != lastValue)
            {
                distance?.Invoke(dist);
                lastValue = dist;
            }
        }
    }

    protected void RestrictRange()
    {
        if (distanceRange.x > distanceRange.y)
            distanceRange.y = distanceRange.x;
    }

    public void SetOrigin(Transform transform)
    {
        origin = new ObjectRef<Transform>("Origin", transform);
    }

    public void SetTarget(Transform transform)
    {
        target = new ObjectRef<Transform>("Target", transform);
    }
}
