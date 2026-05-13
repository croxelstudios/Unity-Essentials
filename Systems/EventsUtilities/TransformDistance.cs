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
        Vector3 other = ((Transform)target).position;
        if (local)
            other = ((Transform)origin).InverseTransformPoint(other);
        float dist = Vector3.Distance(local ? ((Transform)origin).localPosition : ((Transform)origin).position, other);
        if (lerpDistance)
            dist = Mathf.InverseLerp(distanceRange.x, distanceRange.y, dist);
        distance?.Invoke(dist * multiplier);
    }

    public void RestrictRange()
    {
        if (distanceRange.x > distanceRange.y)
            distanceRange.y = distanceRange.x;
    }
}
