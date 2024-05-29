using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class RenderersSetFloat4 : BRenderersSetProperty
{
	public Vector4 value = new Vector4(1f, 1f, 0f, 0f);

	Vector4 oldValue;
	static Dictionary<RendererMaterial, Vector4> originals;
    public static CustomEvent init;
	public class CustomEvent : UnityEvent<RenderersSetFloat4> { }

    void Reset()
    {
        propertyName = "_MainTex_ST";
    }

    protected override void Init()
    {
        init?.Invoke(this);
        oldValue = value;
        //TO DO: Should work on a stack like the colors maybe
	    originals = new Dictionary<RendererMaterial, Vector4>();
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
        RendererMaterial rendMat = new RendererMaterial(rend, mat, propertyName);
        if (!originals.ContainsKey(rendMat))
	        originals.Add(rendMat, block.GetVector(propertyName));
	    block.SetVector(propertyName, value);
    }

    protected override void BlResetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        RendererMaterial rendMat = new RendererMaterial(rend, mat, propertyName);
        if (originals.ContainsKey(rendMat)) block.SetVector(propertyName, originals[rendMat]);
    }

    protected override void VSetProperty(Renderer rend, int mat)
    {
        RendererMaterial rendMat = new RendererMaterial(rend, mat, propertyName);
        if (!originals.ContainsKey(rendMat))
	        originals.Add(rendMat, rend.materials[mat].GetVector(propertyName));
        rend.materials[mat].SetVector(propertyName, value);
    }

    protected override void VResetProperty(Renderer rend, int mat)
    {
        RendererMaterial rendMat = new RendererMaterial(rend, mat, propertyName);
        if (originals.ContainsKey(rendMat))
            rend.materials[mat].SetVector(propertyName, originals[rendMat]);
        base.VResetProperty(rend, mat);
    }

    public void SetX(float n)
    {
        value = new Vector4(n, value.y, value.z, value.w);
    }

    public void SetY(float n)
    {
        value = new Vector4(value.x, n, value.z, value.w);
    }

    public void SetZ(float n)
    {
        value = new Vector4(value.x, value.y, n, value.w);
    }

    public void SetW(float n)
    {
        value = new Vector4(value.x, value.y, value.z, n);
    }
}
