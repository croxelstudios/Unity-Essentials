using System.Collections;
using UnityEngine;

public class PeriodicEvent : MonoBehaviour
{
    [SerializeField]
    [Min(0.02f)]
    protected float _seconds = 0.02f;
    public float seconds
    {
        get { return _seconds; }
        set { _seconds = value; }
    }
    [SerializeField]
    float _speed = 1f;
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }
    [SerializeField]
    DXEvent actions = null;
    [SerializeField]
    bool unscaledTime = false;
    [SerializeField]
    bool waitFirstCycle = false;

    Coroutine co;

    protected virtual void OnEnable()
    {
        co = StartCoroutine(LaunchPeriodicEvent());
    }

    void OnDisable()
    {
        if (co != null) StopCoroutine(co);
    }

    public void SetSeconds(float newSeconds) //TO DO: Should be a variable setter
    {
        _seconds = newSeconds;
    }

    IEnumerator LaunchPeriodicEvent()
    {
        float t;

        if (waitFirstCycle) t = seconds;
        else t = 0f;

        while (true)
        {
            t -= speed * (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            if (t <= 0f)
            {
                NewPeriod();
                t = seconds;
            }
            yield return null;
        }
    }

    protected virtual void NewPeriod()
    {
        actions?.Invoke();
    }
}
