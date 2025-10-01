using Sirenix.OdinInspector;
using UnityEngine;

public class TransformDistance : MonoBehaviour
{
    [SerializeField]
    ObjectRef<Transform> origin = new ObjectRef<Transform>("Origin", "Player");
    [SerializeField]
    ObjectRef<Transform> target = new ObjectRef<Transform>("Target", "MainCamera");
    [SerializeField]
    [OnValueChanged("RestrictRange")]
    Vector2 distanceRange = new Vector2(0f, 10f);
    [SerializeField]
    float multiplier = 1f;
    [SerializeField]
    DXFloatEvent distance = new DXFloatEvent();

    void Reset()
    {
        origin = new ObjectRef<Transform>("Origin", transform);
    }

    void Update()
    {
        float lerpedDistance = Mathf.InverseLerp(distanceRange.x, distanceRange.y,
            Vector3.Distance(((Transform)origin).position, ((Transform)target).position));
        distance?.Invoke(lerpedDistance * multiplier);
    }

    public void RestrictRange()
    {
        if (distanceRange.x > distanceRange.y)
            distanceRange.y = distanceRange.x;
    }
}
