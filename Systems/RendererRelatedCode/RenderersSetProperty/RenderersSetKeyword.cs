using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RenderersSetKeyword : MonoBehaviour
{
    protected Renderer[] rend;

    [SerializeField]
    protected bool affectsChildren = true;
    [SerializeField]
    [Tooltip("It will be applied to all materials if set to a negative number")]
    protected int materialIndex = -1;
    [SerializeField]
    protected string propertyName = "_EMISSION";
    [SerializeField]
    protected bool updateRenderers = false;
    public bool enable = true;
    [SerializeField]
    bool waitOneFrameForInit = false;
    [SerializeField]
    protected RenderingTimeModeOrOnEnable timeMode = RenderingTimeModeOrOnEnable.Update;

    bool oldValue;
    static Dictionary<RendererMaterial, bool> originals;
    public static CustomEvent init;
    public class CustomEvent : UnityEvent<RenderersSetKeyword> { }

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
        init?.Invoke(this);
        oldValue = enable;
        //TO DO: Should work on a stack like the colors maybe
        originals = new Dictionary<RendererMaterial, bool>();
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

    void LateUpdate()
    {
        if (timeMode.IsSmooth())
            UpdateBehaviour();
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
            if (rend[i] != null)
            {
                if (materialIndex < 0)
                    for (int j = 0; j < rend[i].sharedMaterials.Length; j++)
                        UpdateMaterial(i, j, reset);
                else if (materialIndex < rend[i].sharedMaterials.Length)
                    UpdateMaterial(i, materialIndex, reset);
            }
        }
    }

    void UpdateMaterial(int rendId, int materialId, bool reset = false)
    {
        if ((rend[rendId].sharedMaterials[materialId] != null))
        {
            if (reset) VResetProperty(rend[rendId], materialId);
            else VSetProperty(rend[rendId], materialId);
        }
    }

    protected virtual void OnUpdatingProperty()
    {

    }

    protected virtual void VSetProperty(Renderer rend, int mat)
    {
        RendererMaterial rendMat = new RendererMaterial(rend, mat);
        Material m = rend.materials[mat];
        if (!originals.ContainsKey(rendMat))
            originals.Add(rendMat, m.IsKeywordEnabled(propertyName));
        SetKeyword(m, propertyName, enable);
    }

    protected virtual void VResetProperty(Renderer rend, int mat)
    {
        RendererMaterial rendMat = new RendererMaterial(rend, mat);
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

    protected struct RendererMaterial
    {
        public Renderer rend;
        public int mat;

        public RendererMaterial(Renderer rend, int mat)
        {
            this.rend = rend;
            this.mat = mat;
        }
    }
}
