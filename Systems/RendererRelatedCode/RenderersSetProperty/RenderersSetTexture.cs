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

    static Dictionary<RendMatProp, Texture> originals;

    protected override void Init()
    {
        originals = new Dictionary<RendMatProp, Texture>();
        base.Init();
    }

    void Reset()
    {
        propertyName = "_MainTex";
    }

    protected override void BlSetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        if (texture != null)
        {
            RendMatProp rendMat = new RendMatProp(rend, mat, propertyName);
            if (!originals.ContainsKey(rendMat))
                originals.Add(rendMat, block.GetTexture(propertyName));
            block.SetTexture(propertyName, texture);
        }
    }

    protected override void BlResetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        RendMatProp rendMat = new RendMatProp(rend, mat, propertyName);
        if (originals.ContainsKey(rendMat) && (originals[rendMat] != null))
            block.SetTexture(propertyName, originals[rendMat]); //TO DO: This won't reset if the texture is null
    }

    protected override void VSetProperty(Renderer rend, int mat)
    {
        if (texture != null)
        {
            RendMatProp rendMat = new RendMatProp(rend, mat, propertyName);
            if (!originals.ContainsKey(rendMat))
                originals.Add(rendMat, rend.materials[mat].GetTexture(propertyName));
            rend.materials[mat].SetTexture(propertyName, texture);
        }
    }

    protected override void VResetProperty(Renderer rend, int mat)
    {
        RendMatProp rendMat = new RendMatProp(rend, mat, propertyName);
        if (originals.ContainsKey(rendMat))
            rend.materials[mat].SetTexture(propertyName, originals[rendMat]);
        base.VResetProperty(rend, mat);
    }
}
