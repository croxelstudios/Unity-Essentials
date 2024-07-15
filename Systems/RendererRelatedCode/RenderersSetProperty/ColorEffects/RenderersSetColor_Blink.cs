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

    Coroutine co;

    protected override void Init()
    {
        color = defaultColor;
        timeMode = RenderingTimeModeOrOnEnable.OnEnable;
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
                color = defaultColor;
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
            Color newColor = color;
            newColor.r = Mathf.SmoothDamp(color.r, targetColor.r, ref tempSpd.x, smoothTime);
            newColor.g = Mathf.SmoothDamp(color.g, targetColor.g, ref tempSpd.y, smoothTime);
            newColor.b = Mathf.SmoothDamp(color.b, targetColor.b, ref tempSpd.z, smoothTime);
            newColor.a = Mathf.SmoothDamp(color.a, targetColor.a, ref tempSpd.w, smoothTime);
            color = newColor;
            UpdateBehaviour();

            time -= timeMode.DeltaTime();
        }
        co = StartCoroutine(EndBlink(smoothTime));
    }

    IEnumerator EndBlink(float smoothTime)
    {
        Vector4 tempSpd = Vector4.zero;
        while (Mathf.Abs((defaultColor - color).grayscale) > 0f)
        {
            yield return null;
            Color newColor = color;
            newColor.r = Mathf.SmoothDamp(color.r, defaultColor.r, ref tempSpd.x, smoothTime);
            newColor.g = Mathf.SmoothDamp(color.g, defaultColor.g, ref tempSpd.y, smoothTime);
            newColor.b = Mathf.SmoothDamp(color.b, defaultColor.b, ref tempSpd.z, smoothTime);
            newColor.a = Mathf.SmoothDamp(color.a, defaultColor.a, ref tempSpd.w, smoothTime);
            color = newColor;
            UpdateBehaviour();
        }
    }

    public override void SetColor(Color color)
    {
        targetColor = color;
    }
}
