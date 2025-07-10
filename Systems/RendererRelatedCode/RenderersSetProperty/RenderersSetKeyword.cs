using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderersSetKeyword : MonoBehaviour
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
    protected string propertyName = "_EMISSION";
    [SerializeField]
    protected bool updateRenderers = false;
    [OnValueChanged("UpdateBehaviour")]
    public bool enable = true;
    [SerializeField]
    bool waitOneFrameForInit = false;

    bool oldValue;
    static Dictionary<RendMat, bool> originals;

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
        oldValue = enable;
        originals = new Dictionary<RendMat, bool>();
        UpdateRenderers();
        if (IsInitialized()) SetBlocksProperty();
    }

    protected bool IsInitialized()
    {
        return (rend != null) && (rend.Length > 0);
    }

    public virtual void UpdateRenderers()
    {
        if (this.IsActiveAndEnabled())
        {
            if (affectsChildren)
                rend = GetComponentsInChildren<Renderer>(true);
            else
            {
                Renderer r = GetComponent<Renderer>();
                if (r != null) rend = new Renderer[] { r };
            }
        }
    }

    protected virtual void OnDisable()
    {
        if (IsInitialized())
            SetBlocksProperty(true);
    }

    protected virtual void UpdateBehaviour()
    {
        if (enable != oldValue)
        {
            if (updateRenderers) UpdateRenderers();
            if (IsInitialized()) SetBlocksProperty();
            oldValue = enable;
        }
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
        if (mat != null)
        {
            if (reset) VResetProperty(rend, materialId);
            else VSetProperty(rend, materialId);
        }
    }

    protected virtual void OnUpdatingProperty()
    {

    }

    protected virtual void VSetProperty(Renderer rend, int mat)
    {
        RendMat rendMat = new RendMat(rend, mat);
        Material m = rend.materials[mat];
        if (!originals.ContainsKey(rendMat))
            originals.Add(rendMat, m.IsKeywordEnabled(propertyName));
        SetKeyword(m, propertyName, enable);
    }

    protected virtual void VResetProperty(Renderer rend, int mat)
    {
        RendMat rendMat = new RendMat(rend, mat);
        Material m = rend.materials[mat];
        if (originals.ContainsKey(rendMat))
            SetKeyword(m, propertyName, originals[rendMat]);
    }

    public void Set(bool affectsChildren, int materialIndex, string propertyName, bool update, bool updateRenderers)
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

    void SetKeyword(Material mat, string keyword, bool value)
    {
        if (value) mat.EnableKeyword(keyword);
        else mat.DisableKeyword(keyword);
    }

    public void SetKeyword(bool value)
    {
        enable = value;
        UpdateBehaviour();
    }

    public void Enable()
    {
        SetKeyword(true);
    }

    public void Disable()
    {
        SetKeyword(false);
    }
}
