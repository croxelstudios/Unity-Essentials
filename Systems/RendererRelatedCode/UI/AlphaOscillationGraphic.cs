using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class AlphaOscillationGraphic : MonoBehaviour
{
    CanvasGroup canvasGroup;

    [SerializeField]
    Vector2 range = new Vector2(0.5f, 1f);
    [SerializeField]
    [Tooltip("Cycles per second")]
    float speed = 1f;
    [SerializeField]
    [Range(0f, 1f)]
    float startTime = 0f;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;

    float currentAngle;
    float originalAlpha;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalAlpha = canvasGroup.alpha;
    }

    void OnEnable()
    {
        currentAngle = startTime * 360f;
    }

    void OnDisable()
    {
        canvasGroup.alpha = originalAlpha;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        Vector2 newRange = new Vector2(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y));
        range = new Vector2(Mathf.Clamp(newRange.x, 0f, 1f), Mathf.Clamp(newRange.y, 0f, 1f));
    }
#endif

    void Update()
    {
        if (timeMode.IsSmooth()) UpdateAlpha(timeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) UpdateAlpha(Time.fixedDeltaTime);
    }

    void UpdateAlpha(float deltaTime)
    {
        float currentValue = (SineWave(ref currentAngle, deltaTime * speed) + 1f) * 0.5f;
        canvasGroup.alpha = Mathf.Lerp(range.x, range.y, currentValue);
    }

    float SineWave(ref float currentAngle, float deltaTime)
    {
        currentAngle = Mathf.Repeat(currentAngle + (360f * deltaTime), 360f);
        return Mathf.Sin(currentAngle * Mathf.Deg2Rad);
    }
}
