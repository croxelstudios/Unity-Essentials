using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class PeriodicEvent : MonoBehaviour
{
    [SerializeField]
    Randomizable secs = new Randomizable("Seconds", 0.02f, 0.02f);
    [SerializeField]
    [ShowIf("@secs.randomize")]
    bool randomizeOnlyOnEnable = false;
    public float seconds
    {
        get { return randomizeOnlyOnEnable ? secs : secs.Reset(); }
        set { secs.SetValue(value); }
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
