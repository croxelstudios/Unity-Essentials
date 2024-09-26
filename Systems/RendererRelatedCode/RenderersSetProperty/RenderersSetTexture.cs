using UnityEngine;

[ExecuteAlways]
public class RenderersSetTexture : BRenderersSetProperty
{
    [SerializeField]
    Texture _texture = null;
    public Texture texture { get { return _texture; } set { _texture = value; } }

    //Texture oldTexture;

    protected override void Init()
    {
        //oldTexture = texture;
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
            block.SetTexture(propertyName, texture);
    }

    protected override void BlResetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        //TO DO
    }

    protected override void VSetProperty(Renderer rend, int mat)
    {
        rend.materials[mat].SetTexture(propertyName, texture);
    }

    protected override void VResetProperty(Renderer rend, int mat)
    {
        //TO DO
        base.VResetProperty(rend, mat);
    }
}
