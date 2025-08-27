using UnityEngine;
using System.Collections;

public class SlightTilting2D : MonoBehaviour
    // TO DO: Add present extra functionality to BSinoidalTransform and implement base class here
{
    [SerializeField]
    Transform transformOverride = null;
    public float amount = 1f;
    [Tooltip("Cycles per second")]
    public float _speed = 1f;
    public float speed { get { return _speed; } set { _speed = value; } }
    [Range(0f, 1f)]
    public float startTime = 0f;
    [SerializeField]
    float smoothTime = 0.1f;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;
    [SerializeField]
    bool useGlobalTime = false;
    [SerializeField]
    bool playOnEnable = true;
    [SerializeField]
    bool applyToParent = false;

    float currentAmount;
    float currentAngle;
    float currentValue;
    Coroutine smoothCo;
    Coroutine timeCo;
    float tmpSpd;

    void Awake()
    {
        if (transformOverride == null) transformOverride = transform;
        if (applyToParent) transformOverride = transformOverride.parent;
        currentAngle = startTime * 360f;
        currentValue = 0f;
    }

    void OnEnable()
    {
        if (playOnEnable) Play();
    }

    void OnDisable()
    {
        if (smoothCo != null) StopCoroutine(smoothCo);
        if (timeCo != null) StopCoroutine(timeCo);
        Stop();
    }

    void Update()
    {
        if (timeMode.IsSmooth()) UpdatePosition(timeMode.DeltaTime(), timeMode.Time());
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) UpdatePosition(Time.fixedDeltaTime, Time.fixedTime);
    }

    void UpdatePosition(float deltaTime, float time)
    {
        if (currentAmount != 0f)
        {
            float dif = useGlobalTime ? (SineWave((time + startTime) * speed) * currentAmount) - currentValue :
                (SineWave(ref currentAngle, deltaTime * speed) * currentAmount) - currentValue;
            currentValue += dif;
            transformOverride.localEulerAngles += Vector3.forward * dif;
        }
    }

    float SineWave(ref float currentAngle, float deltaTime)
    {
        currentAngle = Mathf.Repeat(currentAngle + (360f * deltaTime), 360f);
        return Mathf.Sin(currentAngle * Mathf.Deg2Rad);
    }

    float SineWave(float currentTime)
    {
        float currentAngle = Mathf.Repeat(360f * currentTime, 360f);
        return Mathf.Sin(currentAngle * Mathf.Deg2Rad);
    }

    public void Play()
    {
        if (this.IsActiveAndEnabled())
        {
            if (smoothCo != null) StopCoroutine(smoothCo);
            smoothCo = StartCoroutine(SmoothCurrentAmount(amount, smoothTime, Mathf.Epsilon, timeMode));
        }
    }

    public void Stop()
    {
        if (this.IsActiveAndEnabled())
        {
            if (smoothCo != null) StopCoroutine(smoothCo);
            smoothCo = StartCoroutine(SmoothCurrentAmount(0f, smoothTime, Mathf.Epsilon, timeMode));
        }
    }

    public void PlayForSeconds(float time)
    {
        if (this.IsActiveAndEnabled())
        {
            Play();
            if (timeCo != null) StopCoroutine(timeCo);
            timeCo = StartCoroutine(StopAfterSeconds(time));
        }
    }

    IEnumerator StopAfterSeconds(float time)
    {
        yield return new WaitForSeconds(time);
        Stop();
    }

    IEnumerator SmoothCurrentAmount(float target, float smoothTime, float minValue, TimeMode timeMode)
    {
        while (Mathf.Abs(currentAmount - target) > minValue)
        {
            if (timeMode.IsFixed()) yield return new WaitForFixedUpdate();
            else yield return null;
            currentAmount = Mathf.SmoothDamp(currentAmount, target, ref tmpSpd, smoothTime, Mathf.Infinity, timeMode.DeltaTime());
        }
    }
}
