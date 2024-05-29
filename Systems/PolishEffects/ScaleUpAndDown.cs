using UnityEngine;

public class ScaleUpAndDown : MonoBehaviour
{
    [SerializeField]
    Vector3 amount = Vector3.one * 0.1f;
    [SerializeField]
    [Tooltip("Cycles per second")]
    float speed = 1f;
    [SerializeField]
    [Range(0f, 1f)]
    float startTime = 0f;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;

    float currentAngle;
    float currentValue;

    void Awake()
    {
        currentAngle = startTime * 360f;
        currentValue = 0f;
    }

    void Update()
    {
        if (timeMode.IsSmooth()) UpdatePosition(timeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) UpdatePosition(Time.fixedDeltaTime);
    }

    void UpdatePosition(float deltaTime)
    {
        float dif = SineWave(ref currentAngle, deltaTime * speed) - currentValue;
        currentValue += dif;
        transform.localScale += dif * amount;
    }

    float SineWave(ref float currentAngle, float deltaTime)
    {
        currentAngle = Mathf.Repeat(currentAngle + (360f * deltaTime), 360f);
        return Mathf.Sin(currentAngle * Mathf.Deg2Rad);
    }
}
