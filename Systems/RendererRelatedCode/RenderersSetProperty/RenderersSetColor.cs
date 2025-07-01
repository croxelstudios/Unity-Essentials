using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
public class RenderersSetColor : BRenderersSetProperty
{
    static Dictionary<RendererMaterial, List<RenderersSetColor>> stackDictionary;
    //static bool dicWasCleared;
    public BlendMode blendMode = BlendMode.Multiply;
    [SerializeField]
    Color _color = Color.white;
    public Color color { get { return _color; } protected set { _color = value; } }
    Color oldColor;
    BlendMode oldBlendMode;

    public enum BlendMode { Multiply, Average }

    void Reset()
    {
        propertyName = "_BaseColor";
    }

    protected override void Init()
    {
        stackDictionary = stackDictionary.CreateIfNull();
        oldColor = color;
        oldBlendMode = blendMode;
        base.Init();
    }

    protected override void OnDisable()
    {
        StopAllCoroutines();
        if (rend != null)
        {
            foreach (Renderer ren in rend)
            {
                if (ren != null)
                {
                    Material[] shM = ren.sharedMaterials;
                    for (int i = 0; i < shM.Length; i++)
                    {
                        RendererMaterial renMat = new RendererMaterial(ren, i, propertyName);
                        stackDictionary.SmartRemove(renMat, this);
                    }
                }
            }
            base.OnDisable();
        }
    }

    protected override void UpdateBehaviour()
    {
        if ((color != oldColor) || (blendMode != oldBlendMode))
        {
            base.UpdateBehaviour();
            oldColor = color;
            oldBlendMode = blendMode;
        }
        //dicWasCleared = false;
        //StartCoroutine(DictionaryCleanUp());
    }

    protected override void BlSetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        RendererMaterial rendMat = new RendererMaterial(rend, mat, propertyName);

        stackDictionary = stackDictionary.CreateAdd(rendMat, this);

        ApplyFullStackColor(block, rendMat);
    }

    protected override void BlResetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        RendererMaterial rendMat = new RendererMaterial(rend, mat, propertyName);
        if ((stackDictionary != null) && stackDictionary.ContainsKey(rendMat))
            ApplyFullStackColor(block, rendMat);
    }

    protected override void VSetProperty(Renderer rend, int mat)
    {
        RendererMaterial rendMat = new RendererMaterial(rend, mat, propertyName);

        stackDictionary = stackDictionary.CreateAdd(rendMat, this);

        ApplyFullStackColor(rendMat);
    }

    protected override void VResetProperty(Renderer rend, int mat)
    {
        RendererMaterial rendMat = new RendererMaterial(rend, mat, propertyName);
        if ((stackDictionary != null) && stackDictionary.ContainsKey(rendMat))
            ApplyFullStackColor(rendMat);
        base.VResetProperty(rend, mat);
    }

    void ApplyFullStackColor(MaterialPropertyBlock block, RendererMaterial rendMat)
    {
        block.SetColor(propertyName, GetFullStackColor(rendMat));
    }

    void ApplyFullStackColor(RendererMaterial rendMat)
    {
        rendMat.rend.materials[rendMat.mat].SetColor(propertyName, GetFullStackColor(rendMat));
    }

    Color GetFullStackColor(RendererMaterial rendMat)
    {
        Color current = Color.white;
        float alpha = 1f;
        foreach (RenderersSetColor setter in stackDictionary[rendMat])
        {
            switch (setter.blendMode)
            {
                case BlendMode.Average:
                    current += setter.color / stackDictionary[rendMat].Count;
                    break;
                default:
                    current *= setter.color;
                    break;
            }
            alpha *= setter.color.a;
        }
        current.a = alpha;
        return current;
    }

    //IEnumerator DictionaryCleanUp()
    //{
    //    yield return new WaitForEndOfFrame();
    //    if (!dicWasCleared)
    //    {
    //        RendererMaterial[] keys = new RendererMaterial[stackDictionary.Keys.Count];
    //        stackDictionary.Keys.CopyTo(keys, 0);
    //        foreach (RendererMaterial renMat in keys)
    //        {
    //            Material[] shM = renMat.rend.sharedMaterials;
    //            if ((renMat.rend == null) ||
    //                (shM.Length <= renMat.mat) || shM[renMat.mat] == null)
    //                stackDictionary.Remove(renMat);
    //        }
    //        dicWasCleared = true;
    //    }
    //}

    public virtual void SetColor(Color color)
    {
        this.color = color;
        UpdateBehaviour();
    }
}
