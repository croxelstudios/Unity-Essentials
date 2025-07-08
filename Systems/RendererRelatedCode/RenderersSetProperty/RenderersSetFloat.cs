using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RenderersSetFloat : BRenderersSetProperty
{
    public float value = 0.5f;

    float oldValue;
    static Dictionary<RendMatProp, float> originals;

    void Reset()
    {
        propertyName = "_Cutout";
    }

    protected override void Init()
    {
        oldValue = value;
        //TO DO: Should work on a stack like the colors maybe
        originals = new Dictionary<RendMatProp, float>();
        base.Init();
    }

    protected override void UpdateBehaviour()
    {
        if (value != oldValue)
        {
            base.UpdateBehaviour();
            oldValue = value;
        }
    }

    protected override void BlSetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        RendMatProp rendMat = new RendMatProp(rend, mat, propertyName);
        if (!originals.ContainsKey(rendMat))
            originals.Add(rendMat, block.GetFloat(propertyName));
        block.SetFloat(propertyName, value);
    }

    protected override void BlResetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        RendMatProp rendMat = new RendMatProp(rend, mat, propertyName);
        if (originals.ContainsKey(rendMat)) block.SetFloat(propertyName, originals[rendMat]);
    }

    protected override void VSetProperty(Renderer rend, int mat)
    {
        RendMatProp rendMat = new RendMatProp(rend, mat, propertyName);
        if (!originals.ContainsKey(rendMat))
            originals.Add(rendMat, rend.materials[mat].GetFloat(propertyName));
        rend.materials[mat].SetFloat(propertyName, value);
    }

    protected override void VResetProperty(Renderer rend, int mat)
    {
        RendMatProp rendMat = new RendMatProp(rend, mat, propertyName);
        if (originals.ContainsKey(rendMat))
            rend.materials[mat].SetFloat(propertyName, originals[rendMat]);
        base.VResetProperty(rend, mat);
    }

    public virtual void SetFloat(float n)
    {
        value = n;
    }
}
