using UnityEngine;
using System.Collections;

public class TimeSlowdown : MonoBehaviour
{
    [SerializeField]
    float defaultTimeScale = 1f;
    [SerializeField]
    float smoothTime = 0.01f;
    [SerializeField]
    float minDifference = 0.1f;
    [SerializeField]
    bool sleepWhileActive = false;

    static float fixedTimeRelation = -1f;
    static TimeSlowdown staticHolder;
    static Coroutine co;
    bool quitting;

    void Awake()
    {
        if (fixedTimeRelation < 0f) fixedTimeRelation = Time.fixedDeltaTime / Time.timeScale;
    }

    void OnEnable()
    {
        if (sleepWhileActive) SetNewTimeScale(0f);
    }

    void TryCreatingStaticHolder()
    {
        if ((staticHolder == null) && !quitting)
        {
            GameObject tscGO = new GameObject("GlobalTimeScaleManager");
            DontDestroyOnLoad(tscGO);
            staticHolder = tscGO.AddComponent<TimeSlowdown>();
        }
    }

    void OnApplicationQuit()
    {
        quitting = true;
    }

    void OnDisable()
    {
        if ((this == staticHolder) && (co != null)) staticHolder.StopCoroutine(co);
        SetTimeScale(defaultTimeScale);
    }

    public void SetNewTimeScale(float newScale)
    {
        if (this.IsActiveAndEnabled())
        {
            TryCreatingStaticHolder();
            if (staticHolder != null)
            {
                if (co != null) staticHolder.StopCoroutine(co);
                co = staticHolder.StartCoroutine(TransitionToTimeScale(newScale, smoothTime, minDifference));
            }
        }
    }

    public void SetTimeBackToDefault()
    {
        if (this.IsActiveAndEnabled())
        {
            TryCreatingStaticHolder();
            if (staticHolder != null)
            {
                if (co != null) staticHolder.StopCoroutine(co);
                co = staticHolder.StartCoroutine(TransitionToTimeScale(defaultTimeScale, smoothTime, minDifference));
            }
        }
    }

    static void SetTimeScale(float newScale)
    {
        Time.timeScale = newScale;
        Time.fixedDeltaTime = newScale * fixedTimeRelation;
    }

    static IEnumerator TransitionToTimeScale(float newScale, float smoothTime, float mindif)
    {
        float currentScale = Time.timeScale;
        float spd = 0f;
        while (Mathf.Abs(currentScale - newScale) > mindif)
        {
            currentScale = Mathf.SmoothDamp(currentScale, newScale, ref spd,
                smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            SetTimeScale(currentScale);
            yield return null;
        }
        SetTimeScale(newScale);
    }

    public void Sleep(float seconds)
    {
        if (this.IsActiveAndEnabled())
        {
            TryCreatingStaticHolder();
            if (co != null) staticHolder.StopCoroutine(co);
            co = staticHolder.StartCoroutine(SleepCo(seconds, defaultTimeScale));
        }
    }

    static IEnumerator SleepCo(float seconds, float returnToTimeScale)
    {
        SetTimeScale(0f);
        yield return new WaitForSecondsRealtime(seconds);
        SetTimeScale(returnToTimeScale);
    }
}
