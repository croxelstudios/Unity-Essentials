using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(-1000)]
public class RendererDuplicator_Trail : RendererDuplicator
{
    [Header("Trail settings")]
    [SerializeField]
    float time = 0.1f;
    [SerializeField]
    bool useFadeGradient = true;
    [SerializeField]
    Gradient fadeGradient = new Gradient();

    float[] fades;
    TransformData[] states;
    Coroutine co;

    protected override void EnableActions()
    {
        base.EnableActions();
        states = new TransformData[amountOfDuplicates];
        for (int i = 0; i < amountOfDuplicates; i++) states[i] = new TransformData(transform);
        if (useFadeGradient) fades = new float[amountOfDuplicates];
        co = StartCoroutine(UpdateTrail(time));
    }

    protected override void DisableActions()
    {
        base.DisableActions();
        colorSetters = null;
        fades = null;
        StopCoroutine(co);
    }

    public override void UpdateEvent()
    {
        UpdateDuplicateOffsets(objectToDuplicate.transform, duplicates, offsetLocally, tranformOffsetMultipliers);
        for (int i = 0; i < duplicates.Length; i++)
            states[i].GetOffsetFrom(transform).AddTo(duplicates[i].transform);
        if (useFadeGradient)
            for (int i = 0; i < duplicates.Length; i++)
            {
                fades[i] += Time.deltaTime / (time * amountOfDuplicates);
                Color setterColor = fadeGradient.Evaluate(fades[i]);
                if (useAlongGradient) setterColor *= GetColorOfDuplicate(i);
                colorSetters[duplicates[i]].SetColor(setterColor);
            }
    }

    IEnumerator UpdateTrail(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        AddPosition();
        co = StartCoroutine(UpdateTrail(time));
    }

    void AddPosition()
    {
        for (int i = duplicates.Length - 1; i > 0; i--)
        {
            UpdateDuplicate(duplicates[i - 1], ref duplicates[i]);
            states[i] = states[i - 1];
            if (useFadeGradient) fades[i] = fades[i - 1];
        }
        UpdateDuplicate(source, ref duplicates[0]);
        states[0] = new TransformData(transform);
        if (useFadeGradient) fades[0] = 0f;
    }
}
