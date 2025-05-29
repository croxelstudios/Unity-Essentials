using System.Collections;
using UnityEngine;

public class RandomIntervalEvents : MonoBehaviour //TO DO: Should inherit from periodic event random probably
{
    [SerializeField]
    Vector2 timeBetweenIntervalsRange = new Vector2(0.3f, 1f);
    [SerializeField]
    Vector2 intervalsDurationRange = new Vector2(0.01f, 0.1f);
    [SerializeField]
    float _speed = 1f;
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }
    [SerializeField]
    DXEvent startInterval = null;
    [SerializeField]
    DXEvent stopInterval = null;
    [SerializeField]
    [Tooltip("This will ignore randomizing factor and use the minimum values")]
    bool useGlobalTime = false;
    [SerializeField]
    bool unscaledTime = false;
    [SerializeField]
    bool onEnableInterval = false;
    [SerializeField]
    bool onDisableStopsInterval = true;

    Coroutine co;
    bool onInterval;
    float fullDuration;

    void OnEnable()
    {
        if (useGlobalTime)
            fullDuration = timeBetweenIntervalsRange.x + intervalsDurationRange.x;
        else
        {
            if (onEnableInterval) co = StartCoroutine(Interval());
            else co = StartCoroutine(WaitForInterval());
        }
    }

    void Update()
    {
        if (useGlobalTime)
        {
            float current = Mathf.Repeat(unscaledTime ? Time.unscaledTime : Time.time, fullDuration);
            float comparator = onEnableInterval ? intervalsDurationRange.x : timeBetweenIntervalsRange.x;
            if ((current > comparator) ^ onEnableInterval) StartInterval();
            else StopInterval();
        }
    }

    void OnDisable()
    {
        if (co != null) StopCoroutine(co);
        if (onDisableStopsInterval) StopInterval();
    }

    void StartInterval()
    {
        if (!onInterval)
        {
            startInterval?.Invoke();
            onInterval = true;
        }
    }

    void StopInterval()
    {
        if (onInterval)
        {
            stopInterval?.Invoke();
            onInterval = false;
        }
    }

    public void ForceStartInterval()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Interval());
    }

    public void ForceStopInterval()
    {
        if (co != null) StopCoroutine(co);
        StopInterval();
        co = StartCoroutine(WaitForInterval());
    }

    IEnumerator Interval()
    {
        StartInterval();
        float t = Random.Range(intervalsDurationRange.x, intervalsDurationRange.y);
        while (t > 0f)
        {
            yield return null;
            t -= speed * (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }
        StopInterval();
        if (this.IsActiveAndEnabled())
            co = StartCoroutine(WaitForInterval());
    }

    IEnumerator WaitForInterval()
    {
        float t = Random.Range(timeBetweenIntervalsRange.x, timeBetweenIntervalsRange.y);
        while (t > 0f)
        {
            yield return null;
            t -= speed * (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }
        if (this.IsActiveAndEnabled())  
            co = StartCoroutine(Interval());
    }
}
