using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeContext_Clock : MonoBehaviour
{
    Dictionary<int, float> time;
    Dictionary<int, float> fixedTime;

    void EnsureCustomTime(int custom)
    {
        if (custom > 0)
            time = time.CreateAdd(custom, Time.unscaledTime);
    }

    void EnsureCustomFixedTime(int custom)
    {
        if (custom > 0)
            fixedTime = fixedTime.CreateAdd(custom, Time.fixedTime);
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void Update()
    {
        if (!time.IsNullOrEmpty())
            foreach (int custom in time.Keys)
                time[custom] += TimeContext.DeltaTime(custom);
    }

    void FixedUpdate()
    {
        if (!fixedTime.IsNullOrEmpty())
            foreach (int custom in fixedTime.Keys)
                fixedTime[custom] += TimeContext.FixedDeltaTime(custom);
    }

    public float GetCustomTime(int custom)
    {
        if (custom <= 0)
            return Time.time;

        EnsureCustomTime(custom);
        return time[custom];
    }

    public float GetCustomFixedTime(int custom)
    {
        if (custom <= 0)
            return Time.time;

        EnsureCustomFixedTime(custom);
        return fixedTime[custom];
    }

    public Coroutine StartWaitFor(int custom)
    {
        return StartCoroutine(WaitFor(custom));
    }

    public Coroutine StartWaitForFixed(int custom)
    {
        return StartCoroutine(WaitForFixed(custom));
    }

    IEnumerator WaitFor(int custom)
    {
        if (custom > 0)
            yield return new WaitForSeconds(TimeContext.GetTimeScale(custom));
        else yield return null;
    }

    IEnumerator WaitForFixed(int custom)
    {
        if (custom > 0)
        {
            float time = TimeContext.GetTimeScale(custom);
            while (time > 0f)
            {
                time -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }
        else yield return new WaitForFixedUpdate();
    }
}
