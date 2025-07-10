using Sirenix.OdinInspector;
using UnityEngine;

public class RenderersSetTexture_FromMaterial : RenderersSetTexture
{
    [Space]
    [SerializeField]
    [InlineProperty]
    [HideLabel]
    ProceduralTexture proceduralTexture = new ProceduralTexture(null);
    [SerializeField]
    RenderingTimeModeOrOnEnable timeMode = RenderingTimeModeOrOnEnable.Update;

    protected override void OnEnable()
    {
        if (timeMode == RenderingTimeModeOrOnEnable.OnEnable)
            UpdateRT();
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        proceduralTexture.Release();
    }

    void LateUpdate()
    {
        if (timeMode.IsSmooth())
            UpdateBehaviour();
    }

    protected override void UpdateBehaviour()
    {
        UpdateRT();
        base.UpdateBehaviour();
    }

    void UpdateRT()
    {
        if (proceduralTexture.IsValid())
        {
            proceduralTexture.Update();
            texture = proceduralTexture.rt;
        }
    }
}
