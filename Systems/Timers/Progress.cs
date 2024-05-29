using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Progress : MonoBehaviour
{
    [SerializeField]
    [Min(0.02f)]
    float _duration = 1f;
    public float duration
    {
        get { return _duration; }
        set { _duration = value; }
    }
    [SerializeField]
    float _speed = 1f;
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }
    [SerializeField]
    float currentProgress = 0f;
    [SerializeField]
    DXEvent emptyActions = null;
    [SerializeField]
    DXEvent fullActions = null;
    [SerializeField]
    DXFloatEvent currentPercent = null;
    [SerializeField]
    bool unscaledTime = false;

    void Update()
    {
        if (((Time.timeScale != 0f) || unscaledTime) && (speed != 0f))
        {
            float prevProgress = currentProgress;
            currentProgress += speed * (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            currentProgress = Mathf.Clamp(currentProgress, 0f, duration);

            if (prevProgress != currentProgress)
            {
                currentPercent?.Invoke(Mathf.Lerp(0f, duration, currentProgress));
                if (currentProgress >= duration) fullActions?.Invoke();
                else if (currentProgress <= 0f) emptyActions?.Invoke();
            }
        }
    }

    public void AddPercent(float percent)
    {
        AddSeconds(Mathf.InverseLerp(0f, duration, percent));
    }

    public void AddSeconds(float seconds)
    {
        SetSeconds(currentProgress + seconds);
    }

    public void SetPercent(float percent)
    {
        SetSeconds(Mathf.InverseLerp(0f, duration, percent));
    }

    public void SetSeconds(float seconds)
    {
        currentProgress = Mathf.Clamp(seconds, 0f, duration);
    }

    public void ResetProgress()
    {
        SetSeconds(0f);
    }
}
