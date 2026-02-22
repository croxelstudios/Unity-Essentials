using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteAlways]
public class BRenderersSetProperty : MonoBehaviour
{
    protected Renderer[] rend;

    [SerializeField]
    [OnValueChanged("UpdateRenderers")]
    protected bool affectsChildren = true;
    [SerializeField]
    [OnValueChanged("UpdateBehaviour")]
    [Tooltip("It will be applied to all materials if set to a negative number")]
    protected int materialIndex = -1;
    [SerializeField]
    [OnValueChanged("UpdateBehaviour")]
    protected string propertyName = "";
    [SerializeField]
    [OnValueChanged("UpdateBehaviour")]
    protected string[] propertyNameAlts = null;
    [SerializeField]
    protected bool updateRenderers = false;
    [SerializeField]
    protected bool dontUsePropertyBlock = false;
    [SerializeField]
    bool waitOneFrameForInit = false;

    protected MaterialPropertyBlock block;
    protected bool propertyIsReadOnly = false;
    protected static Dictionary<RendMat, Material> originals;

    protected virtual void OnEnable()
    {
        if (waitOneFrameForInit)
            StartCoroutine(WaitOneFrame());
        else Init();
    }

    IEnumerator WaitOneFrame()
    {
        yield return new WaitForEndOfFrame();
        Init();
    }

    protected virtual void Init()
    {
        block = new MaterialPropertyBlock();
        UpdateRenderersInternal();
        if (IsInitialized()) SetBlocksProperty();
    }

    protected bool IsInitialized()
    {
        return (!rend.IsNullOrEmpty()) && (block != null);
    }

    protected virtual void UpdateRenderersInternal()
    {
        if (affectsChildren)
        {
            Renderer[] rends = GetComponentsInChildren<Renderer>(true);
            RenderersSetProperty_Exclude[] excluders =
                GetComponentsInChildren<RenderersSetProperty_Exclude>(true);
            if (!excluders.IsNullOrEmpty())
            {
                rend = new Renderer[rends.Length - excluders.Length];
                int n = 0;
                for (int i = 0; i < rends.Length; i++)
                {
                    bool isValid = true;
                    for (int j = 0; j < excluders.Length; j++)
                        if (excluders[j].gameObject == rends[i].gameObject)
                        {
                            isValid = false;
                            break;
                        }
                    if (isValid)
                    {
                        rend[n] = rends[i];
                        n++;
                    }
                }
            }
            else rend = rends;
        }
        else
        {
            Renderer r = GetComponent<Renderer>();
            RenderersSetProperty_Exclude e = GetComponent<RenderersSetProperty_Exclude>();
            if ((r != null) && (e == null)) rend = new Renderer[] { r };
        }
    }

    public virtual void UpdateRenderers()
    {
        if (this.IsActiveAndEnabled())
        {
            UpdateRenderersInternal();
            SetBlocksProperty();
        }
    }

    protected virtual void OnDisable()
    {
        if (IsInitialized())
            SetBlocksProperty(true);
    }

    protected virtual bool ShouldUpdate()
    {
        return false;
    }

    protected void TryUpdate()
    {
        if (ShouldUpdate())
            UpdateBehaviour();
    }

    protected virtual void UpdateBehaviour()
    {
        if (updateRenderers) UpdateRenderersInternal();
        if (IsInitialized()) SetBlocksProperty();
    }

    void SetBlocksProperty(bool reset = false)
    {
        OnUpdatingProperty();
        for (int i = 0; i < rend.Length; i++)
        {
            Renderer r = rend[i];
            if (r != null)
            {
                Material[] shM = r.sharedMaterials;
                if (materialIndex < 0)
                    for (int j = 0; j < shM.Length; j++)
                        UpdateMaterial(new RendMat(r, j), reset);
                else if (materialIndex < shM.Length)
                    UpdateMaterial(new RendMat(r, materialIndex), reset);
            }
        }
    }

    //void UpdateMaterial(int rendId, int materialId, bool reset = false)
    //{
    //    UpdateMaterial(rend[rendId], rend[rendId].sharedMaterials[materialId], materialId, reset);
    //}

    void UpdateMaterial(RendMat rendMat, bool reset = false)
    {
        if ((!rendMat.IsNull()) && CheckPropertyName(rendMat.sharedMaterial, out string propName))
        {
            RendMatProp rendMatProp = new RendMatProp(rendMat, propName);
            if (dontUsePropertyBlock || originals.NotNullContainsKey(rendMat))
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                {
                    if (!originals.NotNullContainsKey(rendMat))
                        originals = originals.CreateAdd(rendMat, rendMat.sharedMaterial);

                    if (reset) VResetProperty(rendMatProp);
                    else VSetProperty(rendMatProp);
                }
            }
            else
            {
                rendMat.GetPropertyBlock(block);
                CheckRendererBlocks(rendMat.rend);
                if (reset) BlResetProperty(block, rendMatProp);
                else BlSetProperty(block, rendMatProp);
                rendMat.SetPropertyBlock(block);
            }
        }
    }

    void CheckRendererBlocks(Renderer rend)
    {
        //This was a workaroud for built-in that is no longer necessary for SRPs
        //if (typeof(SpriteRenderer).IsAssignableFrom(rend.GetType()))
        //{   //This is a bit ugly, but unity would reset properties here if this is not done
        //    //I probably need more of these for other highly specific renderers
        //    SpriteRenderer sprRend = (SpriteRenderer)rend;
        //    if (sprRend.sprite != null)
        //    {
        //        block.SetTexture("_MainTex", sprRend.sprite.texture);
        //        block.SetTexture("_BaseMap", sprRend.sprite.texture);
        //    }
        //    block.SetColor("_RendererColor", sprRend.color);
        //    block.SetColor("_BaseColor", sprRend.color);
        //    block.SetVector("_Flip", new Vector4(sprRend.flipX ? 1f : 0f, sprRend.flipY ? 1f : 0f, 0f, 0f));
        //}
    }

    protected virtual void OnUpdatingProperty()
    {

    }

    protected virtual void BlSetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {

    }

    protected virtual void BlResetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {

    }

    protected virtual void VSetProperty(RendMatProp rendMat)
    {

    }

    protected virtual void VResetProperty(RendMatProp rendMat)
    {
        //TO DO: Full material reset must be done when every SetProperty affecting this material has been reseted or not used

        //Material[] shM = rend.sharedMaterials;
        //if (rend.materials[mat] != shM[mat])
        //{
        //    Destroy(rend.materials[mat]);
        //    rend.materials[mat] = shM[mat];
        //}
    }

    public bool CheckPropertyName(Material mat, out string propName)
    {
        if (mat.HasProperty(propertyName))
        {
            propName = propertyName;
            return true;
        }
        else if (!propertyNameAlts.IsNullOrEmpty())
            for (int i = 0; i < propertyNameAlts.Length; i++)
                if (mat.HasProperty(propertyNameAlts[i]))
                {
                    propName = propertyNameAlts[i];
                    return true;
                }
        propName = "";
        return false;
    }

    [ContextMenu("Reset Property Blocks")]
    public void ResetPropertyBlocks()
    {
        block.SmartClear();
        if (rend != null)
            for (int i = 0; i < rend.Length; i++)
            {
                Renderer r = rend[i];
                if (r != null)
                {
                    CheckRendererBlocks(r);
                    Material[] shM = r.sharedMaterials;
                    for (int j = 0; j < shM.Length; j++)
                        r.SetPropertyBlock(block, j);
                }
            }
    }

    public void Set(bool affectsChildren, int materialIndex, string propertyName, bool updateRenderers)
    {
        this.affectsChildren = affectsChildren;
        this.materialIndex = materialIndex;
        this.propertyName = propertyName;
        this.updateRenderers = updateRenderers;
    }

    public void Set(bool affectsChildren, int materialIndex, string propertyName, string[] propertyNameAlts, bool updateRenderers)
    {
        this.affectsChildren = affectsChildren;
        this.materialIndex = materialIndex;
        this.propertyName = propertyName;
        this.propertyNameAlts = propertyNameAlts;
        this.updateRenderers = updateRenderers;
    }

    protected Material GetSharedMaterial()
    {
        int matInd = materialIndex;
        if (matInd < 0) matInd = 0;
        return rend[0].sharedMaterials[matInd];
    }

    protected MaterialPropertyBlock GetCurrentBlockValues()
    {
        int matInd = materialIndex;
        if (matInd < 0) matInd = 0;
        rend[0].GetPropertyBlock(block, matInd);
        return block;
    }
}

[ExecuteAlways]
public class BRenderersSetProperty<T> : BRenderersSetProperty where T : IEquatable<T>
{
    T oldTValue;
    protected virtual T tValue { get { return default; } set { } }

    protected override void Init()
    {
        oldTValue = tValue;
        base.Init();
    }

    protected override bool ShouldUpdate()
    {
        return (!tValue.Equals(oldTValue)) || base.ShouldUpdate();
    }

    protected override void UpdateBehaviour()
    {
        base.UpdateBehaviour();
        oldTValue = tValue;
    }
}

[ExecuteAlways]
public class BRenderersSetBlendedProperty<T> : BRenderersSetProperty<T> where T : IEquatable<T>
{
    static Dictionary<RendMatProp, List<BRenderersSetBlendedProperty<T>>> stackDictionary;
    //static bool dicWasCleared;
    [OnValueChanged("UpdateBehaviour")]
    public BlendMode blendMode = BlendMode.Multiply;
    BlendMode oldBlendMode;
    [OnValueChanged("UpdateBehaviour")]
    public bool blendWithOriginal = false;

    public enum BlendMode { Multiply, Average, Add, Subtract }

    protected override void Init()
    {
        stackDictionary = stackDictionary.CreateIfNull();
        oldBlendMode = blendMode;
        base.Init();
    }

    protected override void OnDisable()
    {
        StopAllCoroutines(); //TO DO??
        if (rend != null)
        {
            foreach (Renderer ren in rend)
                if (ren != null)
                {
                    Material[] shM = ren.sharedMaterials;
                    for (int i = 0; i < shM.Length; i++)
                        if (shM[i] != null)
                        {
                            CheckPropertyName(shM[i], out string propName);
                            RendMatProp renMat = new RendMatProp(ren, i, propName);
                            stackDictionary.SmartRemove(renMat, this);
                        }
                }
            base.OnDisable();
        }
    }

    protected override bool ShouldUpdate()
    {
        return (blendMode != oldBlendMode) || base.ShouldUpdate();
    }

    protected override void UpdateBehaviour()
    {
        base.UpdateBehaviour();
        oldBlendMode = blendMode;
        //dicWasCleared = false;
        //StartCoroutine(DictionaryCleanUp());
    }

    protected override void BlSetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        stackDictionary = stackDictionary.CreateAdd(rendMat, this);

        ApplyFullStack(block, rendMat);
    }

    protected override void BlResetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        if ((stackDictionary != null) && stackDictionary.ContainsKey(rendMat))
            ApplyFullStack(block, rendMat);
    }

    protected override void VSetProperty(RendMatProp rendMat)
    {
        stackDictionary = stackDictionary.CreateAdd(rendMat, this);

        ApplyFullStack(rendMat);
    }

    protected override void VResetProperty(RendMatProp rendMat)
    {
        if ((stackDictionary != null) && stackDictionary.ContainsKey(rendMat))
            ApplyFullStack(rendMat);
        base.VResetProperty(rendMat);
    }

    protected virtual void BlockSet(MaterialPropertyBlock block, T value, string propertyName)
    {

    }

    protected virtual void MaterialSet(Material mat, T value, string propertyName)
    {

    }

    void ApplyFullStack(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        BlockSet(block, GetFullStackColor(rendMat), rendMat.property);
    }

    void ApplyFullStack(RendMatProp rendMat)
    {
        MaterialSet(rendMat.material, GetFullStackColor(rendMat), rendMat.property);
    }

    T GetFullStackColor(RendMatProp rendMat)
    {
        T current = NeutralAdd();
        int count = stackDictionary[rendMat].Count;
        foreach (BRenderersSetBlendedProperty<T> setter in stackDictionary[rendMat])
            if (setter.blendWithOriginal) count++;
        bool first = true;
        foreach (BRenderersSetBlendedProperty<T> setter in stackDictionary[rendMat])
        {
            if (first)
            {
                setter.SetNeutral(ref current);
                first = false;
            }
            setter.CombineValueIn(ref current, count);
            if (setter.blendWithOriginal)
                setter.CombineValueIn(ref current, GetProperty(rendMat), count);
        }
        return current;
    }

    public void SetNeutral(ref T current)
    {
        switch (blendMode)
        {
            case BlendMode.Average:
                current = NeutralAdd();
                break;
            case BlendMode.Add:
                current = NeutralAdd();
                break;
            case BlendMode.Subtract:
                current = NeutralMult();
                break;
            default:
                current = NeutralMult();
                break;
        }
    }

    public void CombineValueIn(ref T current, int count)
    {
        CombineValueIn(ref current, tValue, count);
    }

    public void CombineValueIn(ref T current, T value, int count)
    {
        switch (blendMode)
        {
            case BlendMode.Average:
                current = Combine_Average(current, value, count);
                break;
            case BlendMode.Add:
                current = Combine_Add(current, value);
                break;
            case BlendMode.Subtract:
                current = Combine_Subtract(current, value);
                break;
            default:
                current = Combine_Multiply(current, value);
                break;
        }
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

    protected virtual T NeutralAdd()
    {
        return default;
    }

    protected virtual T NeutralMult()
    {
        return default;
    }

    protected virtual T Combine_Average(T current, T next, int count)
    {
        return current;
    }

    protected virtual T Combine_Multiply(T current, T next)
    {
        return current;
    }

    protected virtual T Combine_Add(T current, T next)
    {
        return current;
    }

    protected virtual T Combine_Subtract(T current, T next)
    {
        return current;
    }

    protected virtual T GetProperty(Material mat, string propertyName)
    {
        return default;
    }

    protected T GetProperty(RendMatProp rendMat)
    {
        if (originals.NotNullContainsKey(rendMat))
            return GetProperty(originals[rendMat], rendMat.property);
        else return GetProperty(rendMat.sharedMaterial, rendMat.property);
    }
}