using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class PostProWeightFromFloatURP : MonoBehaviour
{
    Volume postProVolume;
    [SerializeField]
    float maxValue = 1f;
    [SerializeField]
    float smoothDmp = 0.03f;
    [SerializeField]
    float margin = 0.01f;
    [SerializeField]
    bool invert = false;
    [SerializeField]
    RenderingTimeMode timeMode = RenderingTimeMode.Update;

    float current;
    float tmpspd;
    Coroutine co;

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetWeight(float weight)
    {
        if (this.IsActiveAndEnabled())
        {
            if (postProVolume == null) postProVolume = GetComponent<Volume>();
            if (postProVolume != null)
            {
                current = weight / maxValue;
                if (invert) current = 1f - current;
                StopAllCoroutines();
                StartCoroutine(SetWeight(postProVolume, Mathf.Clamp01(current), margin));
            }
        }
    }

    public void SetWeight(int weight)
    {
        SetWeight((float)weight);
    }

    public void SetWeightInstant(float weight)
    {
        if (this.IsActiveAndEnabled())
        {
            if (postProVolume == null) postProVolume = GetComponent<Volume>();
            current = weight / maxValue;
            if (invert) current = 1f - current;
            postProVolume.weight = current;
        }
    }

    public void SetWeightInstant(int weight)
    {
        SetWeightInstant((float)weight);
    }

    IEnumerator SetWeight(Volume volume, float target, float margin)
    {
        while (Mathf.Abs(volume.weight - target) > margin)
        {
            yield return timeMode.WaitFor();
            volume.weight = Mathf.SmoothDamp(volume.weight, target, ref tmpspd, smoothDmp);
        }
        volume.weight = target;
    }
}
