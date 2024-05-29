using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class DestroyAfterSeconds : MonoBehaviour
{
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;
    [SerializeField]
    bool startOnEnable = false;
    [SerializeField]
    [ShowIf("startOnEnable")]
    float seconds = 2f;
    [SerializeField]
    DXEvent beforeDying = null;

    Coroutine co;

    void OnEnable()
    {
        if (startOnEnable)
            co = StartCoroutine(DestroyAfter(seconds));
    }

    void OnDisable()
    {
        if (co != null) StopCoroutine(co);
    }

    public void Destroy(float waitSeconds)
    {
        if (enabled)
        {
            if (waitSeconds <= 0f)
            {
                beforeDying?.Invoke();
                Destroy(gameObject);
            }
            else
            {
                if (co != null) StopCoroutine(co);
                co = StartCoroutine(DestroyAfter(waitSeconds));
            }
        }
    }

    IEnumerator DestroyAfter(float time)
    {
        float t = time;
        while (t > 0f)
        {
            yield return timeMode.WaitFor();
            t -= timeMode.DeltaTime();
        }
        beforeDying?.Invoke();
        Destroy(gameObject);
    }
}
