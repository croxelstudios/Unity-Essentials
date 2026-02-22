using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteAlways]
public class RenderersSetTexture : BRenderersSetProperty
{
    [SerializeField]
    [DisableIf("propertyIsReadOnly")]
    [OnValueChanged("UpdateBehaviour")]
    protected Texture _texture = null;
    public Texture texture { get { return _texture; } set { _texture = value; UpdateBehaviour(); } }

    static Dictionary<RendMatProp, Texture> originalTextures;

    protected override void Init()
    {
        originalTextures = new Dictionary<RendMatProp, Texture>();
        base.Init();
    }

    void Reset()
    {
        propertyName = "_MainTex";
    }

    protected override void BlSetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        if (texture != null)
        {
            if (!originalTextures.ContainsKey(rendMat))
                originalTextures.Add(rendMat, block.GetTexture(propertyName));
            block.SetTexture(propertyName, texture);
        }
    }

    protected override void BlResetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        if (originalTextures.ContainsKey(rendMat) && (originalTextures[rendMat] != null))
            block.SetTexture(propertyName, originalTextures[rendMat]); //TO DO: This won't reset if the texture is null
    }

    protected override void VSetProperty(RendMatProp rendMat)
    {
        if (texture != null)
        {
            if (!originalTextures.ContainsKey(rendMat))
                originalTextures.Add(rendMat, rendMat.material.GetTexture(rendMat.property));
            rendMat.material.SetTexture(rendMat.property, texture);
        }
    }

    protected override void VResetProperty(RendMatProp rendMat)
    {
        if (originalTextures.ContainsKey(rendMat))
            rendMat.material.SetTexture(rendMat.property, originalTextures[rendMat]);
        base.VResetProperty(rendMat);
    }
}
