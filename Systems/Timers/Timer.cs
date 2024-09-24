using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Sirenix.OdinInspector;

public class Timer : MonoBehaviour
{
    [SerializeField]
    [MinValue(0.02f)]
    protected float seconds = 1f;
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
    bool startOnEnable = false;

    void OnEnable()
    {
        if (startOnEnable) StartTimer();
    }

    void OnDisable()
    {
        StopTimer();
    }

    public void StopTimer()
    {
        StopAllCoroutines();
    }

    public virtual void StartTimer()
    {
        if (this.IsActiveAndEnabled() && gameObject.activeInHierarchy)
            StartCoroutine(Alarm());
    }

    public void ForceInstantAction()
    {
        if (this.IsActiveAndEnabled() && gameObject.activeInHierarchy)
            actions?.Invoke();
    }

    public void SetSeconds(float newDuration)
    {
        seconds = newDuration;
    }

    IEnumerator Alarm()
    {
        float t = seconds;
        if (unscaledTime)
        {
            while (t > 0f)
            {
                yield return null;
                t -= Time.unscaledDeltaTime * speed;
            }
            actions?.Invoke();
        }
        else
        {
            while (t > 0f)
            {
                yield return null;
                t -= Time.deltaTime * speed;
            }
            actions?.Invoke();
        }
    }
}
