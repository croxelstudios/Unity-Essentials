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
            if (typeof(SpriteRenderer).IsAssignableFrom(rend[i].GetType()))
                list.Add(((SpriteRenderer)rend[i]).sprite.texture);
        oldTexs = list.ToArray();
    }

    protected override void UpdateBehaviour()
    {
        int j = 0;
        for (int i = 0; i < rend.Length; i++)
            if (typeof(SpriteRenderer).IsAssignableFrom(rend[i].GetType()))
            {
                Texture tex = ((SpriteRenderer)rend[i]).sprite.texture;
                if (tex != oldTexs[j])
                    //TO DO: This has a problem if you change any sprite to the last renderer's sprite, it won't update
                {
                    oldTexs[j] = tex;
                    _texture = tex;
                    break;
                }
                j++;
            }

        base.UpdateBehaviour();
    }

    protected override void BlSetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        if (typeof(SpriteRenderer).IsAssignableFrom(rend.GetType()))
            _texture = ((SpriteRenderer)rendMat.rend).sprite.texture;

        base.BlSetProperty(block, rendMat);

        foreach (string prop in otherProperties)
            block.SetTexture(prop, texture);
    }

    protected override void VSetProperty(RendMatProp rendMat)
    {
        if (typeof(SpriteRenderer).IsAssignableFrom(rend.GetType()))
            _texture = ((SpriteRenderer)rendMat.rend).sprite.texture;

        base.VSetProperty(rendMat);

        foreach (string prop in otherProperties)
            rendMat.material.SetTexture(prop, texture);
    }
}
