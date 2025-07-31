using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class RenderersSetColor_Blink : RenderersSetColor
{
    [SerializeField]
    float smoothTime = 0.01f;
    [SerializeField]
    float duration = 0.1f;
    [SerializeField]
    Color defaultColor = Color.clear;
    [SerializeField]
    Color targetColor = Color.white;
    [SerializeField]
    RenderingTimeMode timeMode = RenderingTimeMode.Update;

    Coroutine co;

    protected override void Init()
    {
        color = defaultColor;
        base.Init();
    }

    protected override void UpdateBehaviour()
    {
        bool ur = updateRenderers;
        updateRenderers = false;
        base.UpdateBehaviour();
        updateRenderers = ur;
    }

    protected override void OnDisable()
    {
        color = defaultColor;
        base.OnDisable();
    }

    public void SetDuration(float duration)
    {
        this.duration = duration;
    }

    public void Blink()
    {
        if (this.IsActiveAndEnabled())
        {
            if (updateRenderers) UpdateRenderersInternal();
            if (co != null)
            {
                StopCoroutine(co);
                color = defaultColor;
            }
            co = StartCoroutine(StartBlink(duration, smoothTime));
        }
    }

    IEnumerator StartBlink(float time, float smoothTime)
    {
        Color tempSpd = new Color(0, 0, 0, 0);
        while (time > 0f)
        {
            yield return null;
            color = color.SmoothDamp(targetColor, ref tempSpd, smoothTime, timeMode.DeltaTime());
            TryUpdate();

            time -= timeMode.DeltaTime();
        }
        co = StartCoroutine(EndBlink(smoothTime));
    }

    IEnumerator EndBlink(float smoothTime)
    {
        Color tempSpd = new Color(0, 0, 0, 0);
        while ((Mathf.Abs((defaultColor - color).grayscale) > 0f) || (Mathf.Abs(defaultColor.a - color.a) > 0f))
        {
            yield return null;
            color = color.SmoothDamp(defaultColor, ref tempSpd, smoothTime, timeMode.DeltaTime());
            TryUpdate();
        }
    }

    public override void SetColor(Color color)
    {
        targetColor = color;
    }
}
