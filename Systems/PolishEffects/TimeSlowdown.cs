using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TimeSlowdown : MonoBehaviour
{
    [SerializeField]
    [StringSelector("names", false)]
    int timeSpace = 0;
    [SerializeField]
    float defaultTimeScale = 1f;
    [SerializeField]
    float smoothTime = 0.01f;
    [SerializeField]
    bool sleepWhileActive = false;
    protected string[] names { get { return TimeSpaces.Names(); } }

    const float MINDIF = 0.01f;

    static float fixedTimeRelation = -1f;
    static Dictionary<int, TimeSlowdown> staticHolder;
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
        staticHolder = staticHolder.CreateIfNull();
        if ((!staticHolder.ContainsKey(timeSpace)) && !quitting)
        {
            GameObject tscGO = new GameObject("GlobalTimeScaleManager");
            DontDestroyOnLoad(tscGO);
            TimeSlowdown holder = tscGO.AddComponent<TimeSlowdown>();
            holder.timeSpace = timeSpace;
            staticHolder.Add(timeSpace, holder);
        }
    }

    void OnApplicationQuit()
    {
        quitting = true;
    }

    void OnDisable()
    {
        if (staticHolder.Values.Contains(this) && (co != null)) staticHolder[timeSpace].StopCoroutine(co);
        SetTimeScale(defaultTimeScale, timeSpace);
    }

    public void SetNewTimeScale(float newScale)
    {
        if (this.IsActiveAndEnabled())
        {
            TryCreatingStaticHolder();
            if (staticHolder.NotNullContainsKey(timeSpace))
                staticHolder[timeSpace].ToTimeScale(newScale, smoothTime, MINDIF);
        }
    }

    public void SetTimeBackToDefault()
    {
        if (this.IsActiveAndEnabled())
        {
            TryCreatingStaticHolder();
            if (staticHolder.NotNullContainsKey(timeSpace))
                staticHolder[timeSpace].ToTimeScale(defaultTimeScale, smoothTime, MINDIF);
        }
    }

    static void SetTimeScale(float newScale, int timeSpace = 0)
    {
        if (timeSpace <= 0)
        {
            Time.timeScale = newScale;
            Time.fixedDeltaTime = newScale * fixedTimeRelation;
        }
        else TimeContext.SetTimeScale(timeSpace, newScale);
    }

    void ToTimeScale(float newScale, float smoothTime, float mindif)
    {
        if (co != null) staticHolder[timeSpace].StopCoroutine(co);

        if (smoothTime <= Mathf.Epsilon)
            SetTimeScale(newScale, timeSpace);
        else co = StartCoroutine(TransitionToTimeScale(newScale, smoothTime, mindif, timeSpace));
    }

    static IEnumerator TransitionToTimeScale(float newScale, float smoothTime, float mindif, int timeSpace = 0)
    {
        float currentScale = Time.timeScale;
        float spd = 0f;
        while (Mathf.Abs(currentScale - newScale) > mindif)
        {
            currentScale = Mathf.SmoothDamp(currentScale, newScale, ref spd,
                smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            SetTimeScale(currentScale, timeSpace);
            yield return null;
        }
        SetTimeScale(newScale, timeSpace);
    }

    public void Sleep(float seconds)
    {
        if (this.IsActiveAndEnabled())
        {
            TryCreatingStaticHolder();
            if (co != null) staticHolder[timeSpace].StopCoroutine(co);
            co = staticHolder[timeSpace].StartCoroutine(SleepCo(seconds, defaultTimeScale, timeSpace));
        }
    }

    static IEnumerator SleepCo(float seconds, float returnToTimeScale, int timeSpace = 0)
    {
        SetTimeScale(0f, timeSpace);
        yield return new WaitForSecondsRealtime(seconds);
        SetTimeScale(returnToTimeScale, timeSpace);
    }
}
