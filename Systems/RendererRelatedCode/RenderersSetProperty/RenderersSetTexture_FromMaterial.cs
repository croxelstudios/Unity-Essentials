using Sirenix.OdinInspector;
using UnityEngine;

public class RenderersSetTexture_FromMaterial : RenderersSetTexture
{
    [Space]
    [SerializeField]
    [InlineProperty]
    [HideLabel]
    ProceduralTexture proceduralTexture = new ProceduralTexture(null);

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
