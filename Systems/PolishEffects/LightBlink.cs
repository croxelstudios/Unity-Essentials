using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightBlink : MonoBehaviour
{
    Light pointLight;
    
    [SerializeField]
    LightVariationMode mode = LightVariationMode.Range;
    [SerializeField]
    float amount = 0.01f;
    [SerializeField]
    [Tooltip("Cycles per second")]
    float speed = 3f;
    [SerializeField]
    [Range(0f, 1f)]
    float startTime = 0f;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;
    enum LightVariationMode { Range, Intensity }

    float currentAngle;
    float currentValue;

    void Awake()
    {
        pointLight = GetComponent<Light>();
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
        switch (mode)
        {
            case LightVariationMode.Intensity:
                pointLight.intensity += dif * amount;
                break;
            default:
                pointLight.range += dif * amount;
                break;
        }
    }

    float SineWave(ref float currentAngle, float deltaTime)
    {
        currentAngle = Mathf.Repeat(currentAngle + (360f * deltaTime), 360f);
        return Mathf.Sin(currentAngle * Mathf.Deg2Rad);
    }
}
