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
    [HideIf("resetMode", ResetMode.Never)]
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
        return Generics.Scale(current, amountMult);
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
        if (co != null)
        {
            StopCoroutine(co);
            ResetMetaCurrent();
        }
        if (resetMode == ResetMode.OnDisable)
        {
            if (gameObject.activeInHierarchy)
                GoBackToDefault();
            else ResetTransform();
        }
    }

    public void GoBackToDefault()
    {
        ResetValues();
        metaCurrent = Generics.Add(metaCurrent, current);
        current = Default<T>.Value;
        co = StartCoroutine(BackToDefault());
    }

    IEnumerator BackToDefault()
    {
        T def = Default<T>.Value;
        while (Generics.HasMagnitude(metaCurrent))
        {
            yield return timeMode.WaitFor();

            T newCurrent = Generics.SmoothDamp(metaCurrent, def, ref metaCurrentSpd,
                returnSmoothTime, Mathf.Infinity, timeMode.DeltaTime());
            T delta = Generics.Subtract(newCurrent, metaCurrent);
            Transformation(Generics.Scale(delta, amountMult));
            metaCurrent = newCurrent;
        }
        ResetMetaCurrent();
    }

    void ResetMetaCurrent()
    {
        Transformation(Generics.Scale(Generics.Negate(metaCurrent), amountMult));
        metaCurrent = Default<T>.Value;
    }

    protected void ApplyTransformation(T value)
    {
        current = Generics.Add(current, value);
        Transformation(Generics.Scale(value, amountMult));
    }

    protected void SetTransformation(T value)
    {
        ApplyTransformation(Generics.Subtract(value, current));
    }

    protected virtual void ResetValues()
    {
    }

    protected void ResetTransform()
    {
        ResetValues();
        ApplyTransformation(Generics.Negate(current));
    }

    protected virtual void Transformation(T value)
    {
    }

    protected T CurrentOffset()
    {
        return Generics.Scale(Generics.Add(current, metaCurrent), amountMult);
    }
}

public class BOffsetBasedTransformer : MonoBehaviour
{
    public enum ResetMode { OnDisable, OnEnable, Never }
}
