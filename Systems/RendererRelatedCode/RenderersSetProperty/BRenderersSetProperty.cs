using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class BRenderersSetProperty : MonoBehaviour
{
    protected Renderer[] rend;

    [SerializeField]
    protected bool affectsChildren = true;
    [SerializeField]
    [Tooltip("It will be applied to all materials if set to a negative number")]
    protected int materialIndex = -1;
    [SerializeField]
    protected string propertyName = "";
    [SerializeField]
    protected bool updateRenderers = false;
    [SerializeField]
    protected bool dontUsePropertyBlock = false;
    [SerializeField]
    protected RenderingTimeModeOrOnEnable timeMode = RenderingTimeModeOrOnEnable.Update;
    [SerializeField]
    bool waitOneFrameForInit = false;

    MaterialPropertyBlock block;

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
            rend = GetComponentsInChildren<Renderer>(true);
        else
        {
            Renderer r = GetComponent<Renderer>();
            if (r != null) rend = new Renderer[] { r };
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

    void LateUpdate()
    {
        if (timeMode.IsSmooth())
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
                        UpdateMaterial(r, shM[j], j, reset);
                else if (materialIndex < shM.Length)
                    UpdateMaterial(r, shM[materialIndex], materialIndex, reset);
            }
        }
    }

    //void UpdateMaterial(int rendId, int materialId, bool reset = false)
    //{
    //    UpdateMaterial(rend[rendId], rend[rendId].sharedMaterials[materialId], materialId, reset);
    //}

    void UpdateMaterial(Renderer rend, Material mat, int materialId, bool reset = false)
    {
        if ((mat != null) && mat.HasProperty(propertyName))
        {
            if (dontUsePropertyBlock)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                {
                    if (reset) VResetProperty(rend, materialId);
                    else VSetProperty(rend, materialId);
                }
            }
            else
            {
                rend.GetPropertyBlock(block, materialId);
                CheckRendererBlocks(rend);
                if (reset) BlResetProperty(block, rend, materialId);
                else BlSetProperty(block, rend, materialId);
                rend.SetPropertyBlock(block, materialId);
            }
        }
    }

    void CheckRendererBlocks(Renderer rend)
    {
        //This was a workaroud for build-in that no longer is necessary for SRPs
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

    protected virtual void BlSetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {

    }

    protected virtual void BlResetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {

    }

    protected virtual void VSetProperty(Renderer rend, int mat)
    {

    }

    protected virtual void VResetProperty(Renderer rend, int mat)
    {
        //TO DO: Full material reset must be done when every SetProperty affecting this material has been reseted or not used

        //Material[] shM = rend.sharedMaterials;
        //if (rend.materials[mat] != shM[mat])
        //{
        //    Destroy(rend.materials[mat]);
        //    rend.materials[mat] = shM[mat];
        //}
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
