using UnityEngine;
using System.Collections.Generic;

public class RenderersSetTexture_FromSprite : RenderersSetTexture
{
    [SerializeField]
    string[] otherProperties = null;
    [SerializeField]
    RenderingTimeModeOrOnEnable timeMode = RenderingTimeModeOrOnEnable.Update;

#if UNITY_EDITOR
    void OnValidate()
    {
        propertyIsReadOnly = true;
    }
#endif

    void LateUpdate()
    {
        if (timeMode.IsSmooth())
            UpdateBehaviour();
    }

    Texture[] oldTexs;
    protected override void Init()
    {
        base.Init();

        List<Texture> list = new List<Texture>();
        for (int i = 0; i < rend.Length; i++)
            if (rend[i].renType == RenType.Sprite)
                list.Add(((SpriteRenderer)rend[i].renderer).sprite.texture);
        oldTexs = list.ToArray();
    }

    public override void UpdateBehaviour()
    {
        int j = 0;
        if (!rend.IsNullOrEmpty())
            for (int i = 0; i < rend.Length; i++)
                if (rend[i].renType == RenType.Sprite)
                {
                    Texture tex = ((SpriteRenderer)rend[i].renderer).sprite.texture;
                    //if (tex != oldTexs[j])
                        //TO DO: This has a problem if you change any sprite to the last renderer's sprite, it won't update
                    {
                        oldTexs[j] = tex;
                        _texture = tex;
                        break;
                    }
                    //j++;
                }

        base.UpdateBehaviour();
    }

    protected override void BlSetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        Renderizable r = rendMat.rend;
        if (r.renType == RenType.Sprite)
            _texture = ((SpriteRenderer)r.renderer).sprite.texture;

        base.BlSetProperty(block, rendMat);

        foreach (string prop in otherProperties)
            block.SetTexture(prop, texture);
    }

    protected override void VSetProperty(RendMatProp rendMat)
    {
        Renderizable r = rendMat.rend;
        if (r.renType == RenType.Sprite)
            _texture = ((SpriteRenderer)r.renderer).sprite.texture;

        base.VSetProperty(rendMat);

        foreach (string prop in otherProperties)
            rendMat.material.SetTexture(prop, texture);
    }
}
