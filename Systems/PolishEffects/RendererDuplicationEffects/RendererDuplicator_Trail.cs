using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-1000)]
public class RendererDuplicator_Trail : RendererDuplicator
{
    const string foldoutName = "Progressive modifiers";

    [Header("Trail settings")]
    [SerializeField]
    float time = 0.1f;
    [FoldoutGroup(foldoutName)]
    [Header("Transform")]
    [SerializeField]
    TransformData progressiveTransformOffsets = new TransformData();
    [FoldoutGroup(foldoutName)]
    [SerializeField]
    bool progressiveOffsetLocally = true;
    [FoldoutGroup(foldoutName)]
    [Header("Color")]
    [SerializeField]
    bool useFadeGradient = true;
    [FoldoutGroup(foldoutName)]
    [ShowIf("useFadeGradient")]
    [Indent]
    [SerializeField]
    Gradient fadeGradient = new Gradient()
    {
        alphaKeys = new GradientAlphaKey[]{
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(0f, 1f)
        }
    };
    bool usesFadeBehaviour
    { get { return useFadeGradient || progressiveTransformOffsets.IsNotZero(); } }
    //TO DO: Needs progressive transformation options

    float[] fades;
    TransformData[] states;
    Coroutine co;

    protected override void EnableActions()
    {
        base.EnableActions();
        states = new TransformData[amountOfDuplicates];
        for (int i = 0; i < amountOfDuplicates; i++) states[i] = new TransformData(transform);
        fades = new float[amountOfDuplicates];
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
        bool followParent = CheckFollowParent(duplicates[0],
            out TransformData parentData, out Transform parent);

        UpdateDuplicateOffsets(false);

        TransformData progressiveOffsets = new TransformData();
        if (usesFadeBehaviour)
            progressiveOffsets = ProcessOffset(progressiveTransformOffsets);

        for (int i = 0; i < duplicates.Length; i++)
        {
            states[i].GetOffsetFrom(source.transform).AddTo(duplicates[i].transform);
            if (usesFadeBehaviour)
            {
                fades[i] += Time.deltaTime / (time * amountOfDuplicates);
                TransformData off = progressiveOffsets.LerpFromZero(fades[i]);
                UpdateDuplicateOffset(i, duplicates[i], off, progressiveOffsetLocally, false, false);
                //TO DO: There seems to be a bug when activating progressiveOffsetLocally and having
                //scale applied in the per-duplicate offsets (the non-progressive offsets)
            }
            if (useFadeGradient || useAlongGradient)
            {
                Color setterColor = Color.white;
                if (useFadeGradient)
                    setterColor *= fadeGradient.Evaluate(fades[i]);
                if (useAlongGradient)
                    setterColor *= GetColorOfDuplicate(i);
                colorSetters[duplicates[i]].SetColor(setterColor);
            }
        }

        if (followParent)
            parentData.SetInTransform(parent, true);
    }

    IEnumerator UpdateTrail(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        AddPosition();
        co = StartCoroutine(UpdateTrail(time));
    }

    void AddPosition()
    {
        RenderingAgent last = duplicates[duplicates.Length - 1];
        for (int i = duplicates.Length - 1; i > 0; i--)
        {
            duplicates[i] = duplicates[i - 1];
            states[i] = states[i - 1];
            if (usesFadeBehaviour) fades[i] = fades[i - 1];
            AddRendererOrders(source, i, duplicates[i], addSortingOrder, addQueue);
        }
        duplicates[0] = last;
        UpdateDuplicate(source, ref duplicates[0]);
        AddRendererOrders(source, 0, duplicates[0], addSortingOrder, addQueue);
        states[0] = new TransformData(source.transform);
        if (usesFadeBehaviour) fades[0] = 0f;
    }
}
