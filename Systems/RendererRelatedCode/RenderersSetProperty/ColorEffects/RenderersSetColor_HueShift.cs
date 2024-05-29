using UnityEngine;

[ExecuteAlways]
public class RenderersSetColor_HueShift : RenderersSetColor
{
    [Range(0, 1)]
    [SerializeField]
    float saturation = 1f;
    [Range(0, 1)]
    [SerializeField]
    float value = 1f;
    [SerializeField]
    float hueSpeed = 1f;

    float originH;
    protected override void Init()
    {
        Color.RGBToHSV(color, out originH, out float originS, out float originV);
        base.Init();
    }

    protected override void UpdateBehaviour()
    {
        float currentTime = timeMode.Time() * 0.05f;

        Color newColor = Color.HSVToRGB((originH + (currentTime * hueSpeed)) % 1f, saturation, value);

        color = newColor;
        base.UpdateBehaviour();
    }
}
