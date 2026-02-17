using UnityEngine;

[ExecuteAlways]
public class RenderersSetColor_RandomHue : RenderersSetColor
{
    protected override void Init()
    {
        Color newColor = Color.HSVToRGB(Random.value, 1f, 1f);
        newColor.a = 1f;
        color = newColor;
        base.Init();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        propertyIsReadOnly = true;
    }
#endif
}
