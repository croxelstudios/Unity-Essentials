using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteAlways]
public class BRenderersSetProperty : DXMonoBehaviour
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
    protected bool usePropertyBlock = false;
    [SerializeField]
    bool waitOneFrameForInit = false;

    protected MaterialPropertyBlock block;
    protected bool propertyIsReadOnly = false;
    protected static Dictionary<RendMat, Material> originalMaterials;

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
            if (((!usePropertyBlock) || originalMaterials.NotNullContainsKey(rendMat))
#if UNITY_EDITOR
                && Application.isPlaying
#endif
                )
            {
                if (!originalMaterials.NotNullContainsKey(rendMat))
                    originalMaterials = originalMaterials.CreateAdd(rendMat, rendMat.sharedMaterial);

                if (reset) VResetProperty(rendMatProp);
                else VSetProperty(rendMatProp);
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
    [OnValueChanged("UpdateBehaviour")]
    public BlendMode blendMode = BlendMode.Multiply;
    [OnValueChanged("UpdateBehaviour")]
    public bool blendWithOriginal = false;

    BlendMode oldBlendMode;
    bool old_blendWithOriginal = false;
    Dictionary<RendMat, ValueBlender<T>> blender;
    Dictionary<RendMat, ValueBlender<T>> originalBlender;

    protected override void Init()
    {
        oldBlendMode = blendMode;
        old_blendWithOriginal = blendWithOriginal;
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
                            if (blender.NotNullContainsKey(renMat)) blender[renMat].Dispose();
                            if (originalBlender.NotNullContainsKey(renMat)) originalBlender[renMat].Dispose();
                        }
                }
            base.OnDisable();
        }
    }

    protected override bool ShouldUpdate()
    {
        return (blendMode != oldBlendMode) || (blendWithOriginal != old_blendWithOriginal) ||
            base.ShouldUpdate();
    }

    protected override void UpdateBehaviour()
    {
        if (blender != null)
            foreach (RendMat renMat in blender.Keys)
                if (blender[renMat] != null)
                    blender[renMat].Dispose();

        if (originalBlender != null)
            foreach (RendMat renMat in originalBlender.Keys)
                if (originalBlender[renMat] != null)
                    originalBlender[renMat].Dispose();

        base.UpdateBehaviour();
        oldBlendMode = blendMode;
        old_blendWithOriginal = blendWithOriginal;
    }

    protected override void BlSetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        SetUpValueBlender(rendMat);
        ApplyFullStack(block, rendMat);
    }

    protected override void BlResetProperty(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        ApplyFullStack(block, rendMat);
    }

    protected override void VSetProperty(RendMatProp rendMat)
    {
        SetUpValueBlender(rendMat);
        ApplyFullStack(rendMat);
    }

    protected override void VResetProperty(RendMatProp rendMat)
    {
        ApplyFullStack(rendMat);
        base.VResetProperty(rendMat);
    }

    void SetUpValueBlender(RendMatProp rendMat)
    {
        blender = blender.CreateAdd((RendMat)rendMat, (ValueBlender<T>)null);
        blender[rendMat] = blender[rendMat].Set(this, rendMat, PreprocessValue(rendMat, tValue), blendMode);
        if (blendWithOriginal)
        {
            originalBlender = originalBlender.CreateAdd((RendMat)rendMat, (ValueBlender<T>)null);
            originalBlender[rendMat] =
                originalBlender[rendMat].Set(this, rendMat, GetProperty(rendMat), blendMode);
        }
    }

    protected int Count(RendMatProp rendMat)
    {
        if (blender.TryGetValue(rendMat, out ValueBlender<T> bl))
            if (bl != null)
                return bl.Count();
        return 0;
    }

    protected virtual T PreprocessValue(RendMatProp rendMat, T value)
    {
        return value;
    }

    protected virtual void BlockSet(MaterialPropertyBlock block, T value, string propertyName)
    {

    }

    protected virtual void MaterialSet(Material mat, T value, string propertyName)
    {

    }

    void ApplyFullStack(MaterialPropertyBlock block, RendMatProp rendMat)
    {
        BlockSet(block, blender[rendMat].GetBlend(), rendMat.property);
    }

    void ApplyFullStack(RendMatProp rendMat)
    {
        MaterialSet(rendMat.material, blender[rendMat].GetBlend(), rendMat.property);
    }

    protected virtual T GetProperty(Material mat, string propertyName)
    {
        return default;
    }

    protected T GetProperty(RendMatProp rendMat)
    {
        bool fromOriginal = false;
        if (originalMaterials.NotNullContainsKey(rendMat))
        {
            if (originalMaterials[rendMat] == null)
                originalMaterials.Remove(rendMat);
            else fromOriginal = true;
        }

        if (fromOriginal)
            return GetProperty(originalMaterials[rendMat], rendMat.property);
        else return GetProperty(rendMat.sharedMaterial, rendMat.property);
    }
}