using System.Collections;
using UnityEngine;

public class Chronometer : MonoBehaviour
{
    [SerializeField]
    float speed = 1f;
    [SerializeField]
    bool startOnEnable = true;
    [SerializeField]
    bool launchOnUpdate = false;
    [SerializeField]
    bool unscaledTime = false;
    [SerializeField]
    DXFloatEvent result = null;

    bool isRunning;
    float currentTime;
    Coroutine co;

    void OnEnable()
    {
        if (startOnEnable) StartTimer();
    }

    void OnDisable()
    {
        if (co != null) StopAllCoroutines();
    }

    public void StartTimer()
    {
        currentTime = 0f;
        if (co != null) StopAllCoroutines();
        co = StartCoroutine(CountTime());
        isRunning = true;
    }

    public void PauseTimer()
    {
        if (isRunning)
            StopAllCoroutines();
    }

    public void UnpauseTimer()
    {
        if (isRunning && (co == null))
            co = StartCoroutine(CountTime());
    }

    public void StopTimer()
    {
        if (isRunning)
        {
            StopAllCoroutines();
            result?.Invoke(currentTime);
            isRunning = false;
        }
    }

    IEnumerator CountTime()
    {
        while (true)
        {
            yield return null;
            currentTime += (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * speed;
            if (launchOnUpdate) result?.Invoke(currentTime);
        }
    }
}
