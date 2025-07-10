using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class RenderersSetFloat_Blink : RenderersSetFloat
{
    [SerializeField]
    float smoothTime = 0.01f;
    [SerializeField]
    float duration = 0.1f;
    [SerializeField]
    float defaultValue = 0f;
    [SerializeField]
    float targetValue = 1f;
    [SerializeField]
    RenderingTimeMode timeMode = RenderingTimeMode.Update;

    Coroutine co;

    protected override void Init()
    {
        value = defaultValue;
        base.Init();
    }

    protected override void UpdateBehaviour()
    {
        bool ur = updateRenderers;
        updateRenderers = false;
        base.UpdateBehaviour();
        updateRenderers = ur;
    }

    public void Blink()
    {
        if (this.IsActiveAndEnabled())
        {
            if (updateRenderers) UpdateRenderersInternal();
            if (co != null)
            {
                StopCoroutine(co);
                value = defaultValue;
            }
            co = StartCoroutine(StartBlink(duration, smoothTime));
        }
    }

    IEnumerator StartBlink(float time, float smoothTime)
    {
        Vector4 tempSpd = Vector4.zero;
        while (time > 0f)
        {
            yield return null;
            float nValue = value;
            nValue = Mathf.SmoothDamp(value, targetValue, ref tempSpd.x, smoothTime,
                Mathf.Infinity, timeMode.DeltaTime());
            value = nValue;
            UpdateBehaviour();

            time -= timeMode.DeltaTime();
        }
        co = StartCoroutine(EndBlink(smoothTime));
    }

    IEnumerator EndBlink(float smoothTime)
    {
        Vector4 tempSpd = Vector4.zero;
        while (Mathf.Abs(defaultValue - value) > 0f)
        {
            yield return null;
            float nValue = value;
            nValue = Mathf.SmoothDamp(value, defaultValue, ref tempSpd.x, smoothTime,
                Mathf.Infinity, timeMode.DeltaTime());
            value = nValue;
            UpdateBehaviour();
        }
    }

    public override void SetFloat(float value)
    {
        targetValue = value;
    }
}
