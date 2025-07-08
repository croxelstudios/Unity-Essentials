using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

[ExecuteAlways]
public class MaterialsReplacer : MonoBehaviour
{
    [SerializeField]
    int parentSearch = 0;
    [SerializeField]
    MaterialPair[] materialReplacements = null;

    Renderer[] rend;
    Dictionary<RendMat, Material> changedMaterials;
    Dictionary<Material, Material> replacements;
    bool isChanged;

    void UpdateMaterials()
    {
        replacements = replacements.ClearOrCreate();
        foreach (MaterialPair m in materialReplacements)
            replacements.Add(m.replaceThis, m.byReplacement);
    }

    public void UpdateRenderers()
    {
        Transform parent = transform;
        for (int i = 0; i < parentSearch; i++)
            parent = parent.parent;
        rend = parent.GetComponentsInChildren<Renderer>(true);
    }

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        UpdateRenderers();

        isChanged = false;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        ResetMaterials();
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        changedMaterials = changedMaterials.CreateIfNull();
        if (!isChanged)
        {
            UpdateMaterials();
            foreach (Renderer r in rend)
            {
                Material[] shM = r.sharedMaterials;
                for (int i = 0; i < shM.Length; i++)
                    if (replacements.ContainsKey(shM[i]))
                    {
                        changedMaterials.Add(new RendMat(r, i), shM[i]);
                        shM[i] = replacements[shM[i]];
                    }
                r.sharedMaterials = shM;
            }
            isChanged = true;
        }
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        ResetMaterials();
    }

    void ResetMaterials()
    {
        if (isChanged)
        {
            foreach (KeyValuePair<RendMat, Material> kv in changedMaterials)
            {
                Material[] sm = kv.Key.rend.sharedMaterials;
                sm[kv.Key.mat] = kv.Value;
                kv.Key.rend.sharedMaterials = sm;
            }
            changedMaterials.Clear();
            isChanged = false;
        }
    }

    [Serializable]
    struct MaterialPair
    {
        public Material replaceThis;
        public Material byReplacement;

        public MaterialPair(Material replaceThis, Material byReplacement)
        {
            this.replaceThis = replaceThis;
            this.byReplacement = byReplacement;
        }
    }
}
