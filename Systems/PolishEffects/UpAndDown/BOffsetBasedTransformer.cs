using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class BOffsetBasedTransformer<T> : BOffsetBasedTransformer where T : unmanaged
{
    [SerializeField]
    [PropertyOrder(5)]
    protected TimeMode timeMode = TimeMode.Update;
    [SerializeField]
    [PropertyOrder(5)]
    protected ResetMode resetMode = ResetMode.OnDisable;
    [SerializeField]
    [PropertyOrder(5)]
    [ShowIf("@resetMode != BOffsetBasedTransformer.ResetMode.Never")]
    [Indent]
    protected float returnSmoothTime = 0.1f;

    T current;
    T metaCurrent;
    T metaCurrentSpd;
    float amountMult;
    Coroutine co;

    public void SetAmountMultiplier(float value)
    {
        amountMult = value;
    }

    protected T Current()
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

    public void GoBackToDefault()
    {
        metaCurrent = Generics.Add(metaCurrent, current);
        current = Default<T>.Value;
        co = StartCoroutine(BackToDefault());
    }

    IEnumerator BackToDefault()
    {
        while (Generics.HasMagnitude(metaCurrent))
        {
            yield return timeMode.WaitFor();

            T newCurrent = Generics.SmoothDamp(metaCurrent, current, ref metaCurrentSpd,
                returnSmoothTime, Mathf.Infinity, timeMode.DeltaTime());
            Transformation(Generics.Subtract(newCurrent, metaCurrent));
            metaCurrent = newCurrent;
        }
        ResetMetaCurrent();
    }

    void ResetMetaCurrent()
    {
        Transformation(Generics.Negate(metaCurrent));
    }

    protected void ApplyTransform(T value)
    {
        current = Generics.Add(current, value);
        Transformation(Generics.Scale(value, amountMult));
    }

    protected virtual void ResetTransform()
    {
        ApplyTransform(Generics.Scale(Generics.Negate(current), amountMult));
    }

    protected virtual void Transformation(T value)
    {
    }
}

public class BOffsetBasedTransformer : MonoBehaviour
{
    public enum ResetMode { OnDisable, OnEnable, Never }
}
