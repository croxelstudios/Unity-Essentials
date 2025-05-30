using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class RenderersSetTexture : BRenderersSetProperty
{
    [SerializeField]
    Texture _texture = null;
    public Texture texture { get { return _texture; } set { _texture = value; } }

    static Dictionary<RendererMaterial, Texture> originals;
    public static CustomEvent init;
    public class CustomEvent : UnityEvent<RenderersSetTexture> { }

    //Texture oldTexture;

    protected override void Init()
    {
        init?.Invoke(this);
        //oldTexture = texture;
        originals = new Dictionary<RendererMaterial, Texture>();
        base.Init();
    }

    void Reset()
    {
        propertyName = "_MainTex";
    }

    protected override void UpdateBehaviour()
    {
        //if (texture != oldTexture) //TO DO: This causes issues when restarting the editor
        {
            base.UpdateBehaviour();
            //oldTexture = texture;
        }
    }

    protected override void BlSetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        if (texture != null)
        {
            RendererMaterial rendMat = new RendererMaterial(rend, mat, propertyName);
            if (!originals.ContainsKey(rendMat))
                originals.Add(rendMat, block.GetTexture(propertyName));
            block.SetTexture(propertyName, texture);
        }
    }

    protected override void BlResetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        RendererMaterial rendMat = new RendererMaterial(rend, mat, propertyName);
        if (originals.ContainsKey(rendMat)) block.SetTexture(propertyName, originals[rendMat]);
    }

    protected override void VSetProperty(Renderer rend, int mat)
    {
        if (texture != null)
        {
            RendererMaterial rendMat = new RendererMaterial(rend, mat, propertyName);
            if (!originals.ContainsKey(rendMat))
                originals.Add(rendMat, rend.materials[mat].GetTexture(propertyName));
            rend.materials[mat].SetTexture(propertyName, texture);
        }
    }

    protected override void VResetProperty(Renderer rend, int mat)
    {
        RendererMaterial rendMat = new RendererMaterial(rend, mat, propertyName);
        if (originals.ContainsKey(rendMat))
            rend.materials[mat].SetTexture(propertyName, originals[rendMat]);
        base.VResetProperty(rend, mat);
    }
}
