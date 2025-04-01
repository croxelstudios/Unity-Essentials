using UnityEngine;

public class ScaleUpAndDown : MonoBehaviour
{
    [SerializeField]
    Vector3 amount = Vector3.one * 0.1f;
    [SerializeField]
    [Tooltip("Cycles per second")]
    float _speed = 1f;
    public float speed { get { return _speed; } set { _speed = value; } }
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
        if (timeMode.IsSmooth()) UpdateScale(timeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) UpdateScale(Time.fixedDeltaTime);
    }

    void UpdateScale(float deltaTime)
    {
        float dif = SineWave(ref currentAngle, deltaTime * _speed) - currentValue;
        currentValue += dif;
        transform.localScale += dif * amount;
    }

    float SineWave(ref float currentAngle, float deltaTime)
    {
        currentAngle = Mathf.Repeat(currentAngle + (360f * deltaTime), 360f);
        return Mathf.Sin(currentAngle * Mathf.Deg2Rad);
    }
}
