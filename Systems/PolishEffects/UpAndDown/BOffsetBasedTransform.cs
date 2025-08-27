using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class BOffsetBasedTransform : MonoBehaviour
{
    [SerializeField]
    [PropertyOrder(5)]
    protected TimeMode timeMode = TimeMode.Update;
    [SerializeField]
    [PropertyOrder(5)]
    protected ResetMode resetMode = ResetMode.OnDisable;
    [SerializeField]
    [PropertyOrder(5)]
    [ShowIf("@resetMode != ResetMode.Never")]
    [Indent]
    protected float returnSmoothTime = 0.1f;

    protected enum ResetMode { OnDisable, OnEnable, Never }

    float currentSpd;
    float current;
    float metaCurrent;
    float amountMult;
    Coroutine co;

    public void SetAmountMultiplier(float value)
    {
        amountMult = value;
    }

    protected float Current()
    {
        return current;
    }

    protected virtual void Awake()
    {
        amountMult = 1f;
        ResetTransform();
    }

    protected virtual void OnEnable()
    {
        if (resetMode == ResetMode.OnEnable)
            GoBackToDefault();
    }

    protected virtual void OnDisable()
    {
        if (co != null) StopCoroutine(co);
        if (resetMode == ResetMode.OnDisable)
        {
            if (gameObject.activeInHierarchy)
                GoBackToDefault();
            else ResetTransform();
        }
    }

    void GoBackToDefault()
    {
        metaCurrent = current;
        current = 0f;
        co = StartCoroutine(BackToDefault());
    }

    IEnumerator BackToDefault()
    {
        while (metaCurrent > 0f)
        {
            yield return timeMode.WaitFor();

            float newCurrent = Mathf.SmoothDamp(metaCurrent, current, ref currentSpd,
                returnSmoothTime, Mathf.Infinity, timeMode.DeltaTime());
            Transformation(newCurrent - metaCurrent);
            metaCurrent = newCurrent;
        }
        ResetMetaCurrent();
    }

    void ResetMetaCurrent()
    {
        Transformation(-metaCurrent);
    }

    protected void ApplyTransform(float value)
    {
        current += value;
        Transformation(value * amountMult);
    }

    protected virtual void ResetTransform()
    {
        ApplyTransform(-current * amountMult);
    }

    protected virtual void Transformation(float value)
    {
    }
}
