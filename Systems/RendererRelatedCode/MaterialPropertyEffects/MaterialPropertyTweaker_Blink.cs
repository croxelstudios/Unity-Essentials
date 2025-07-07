using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MaterialPropertyTweaker))]
public class MaterialPropertyTweaker_Blink : MonoBehaviour
{
    [SerializeField]
    float smoothTime = 0.01f;
    [SerializeField]
    float duration = 0.1f;
    [SerializeField]
    Color[] targetColors = null;
    [SerializeField]
    Color defaultColor = Color.clear;
    [SerializeField]
    int repetitions = 1;
    [SerializeField]
    RenderingTimeMode timeMode = RenderingTimeMode.Update;

    MaterialPropertyTweaker tweaker;
    Coroutine co;
    int currentReps;

    void OnEnable()
    {
        tweaker = GetComponent<MaterialPropertyTweaker>();
        tweaker.SetColor(defaultColor);
    }

    public void Blink(int colorId)
    {
        if (this.IsActiveAndEnabled())
        {
            currentReps = repetitions;
            if (co != null)
            {
                StopCoroutine(co);
                tweaker.SetColor(defaultColor);
            }
            co = StartCoroutine(StartBlink(duration, smoothTime, colorId));
        }
    }

    IEnumerator StartBlink(float time, float smoothTime, int colorId)
    {
        Vector4 tempSpd = Vector4.zero;
        while (time > 0f)
        {
            yield return null;
            Color newColor = tweaker.GetColor();
            newColor.r = Mathf.SmoothDamp(newColor.r, targetColors[colorId].r, ref tempSpd.x, smoothTime);
            newColor.g = Mathf.SmoothDamp(newColor.g, targetColors[colorId].g, ref tempSpd.y, smoothTime);
            newColor.b = Mathf.SmoothDamp(newColor.b, targetColors[colorId].b, ref tempSpd.z, smoothTime);
            newColor.a = Mathf.SmoothDamp(newColor.a, targetColors[colorId].a, ref tempSpd.w, smoothTime);
            tweaker.SetColor(newColor);

            time -= timeMode.DeltaTime();
        }
        co = StartCoroutine(EndBlink(smoothTime, colorId));
    }

    IEnumerator EndBlink(float smoothTime, int colorId)
    {
        Vector4 tempSpd = Vector4.zero;
        Color newColor = tweaker.GetColor();
        while (Mathf.Abs((defaultColor - newColor).grayscale) > 0f)
        {
            yield return null;
            newColor = tweaker.GetColor();
            newColor.r = Mathf.SmoothDamp(newColor.r, defaultColor.r, ref tempSpd.x, smoothTime);
            newColor.g = Mathf.SmoothDamp(newColor.g, defaultColor.g, ref tempSpd.y, smoothTime);
            newColor.b = Mathf.SmoothDamp(newColor.b, defaultColor.b, ref tempSpd.z, smoothTime);
            newColor.a = Mathf.SmoothDamp(newColor.a, defaultColor.a, ref tempSpd.w, smoothTime);
            tweaker.SetColor(newColor);
        }
        currentReps--;
        if (currentReps > 0)
            co = StartCoroutine(StartBlink(duration, smoothTime, colorId));
    }
}
