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
    Dictionary<Renderer, int[]> changedMaterials;
    Dictionary<Material, Material> replacements;
    Dictionary<Material, Material> replaced;
    bool isChanged;

    void UpdateMaterials()
    {
        replacements = new Dictionary<Material, Material>();
        replaced = new Dictionary<Material, Material>();
        foreach (MaterialPair m in materialReplacements)
        {
            replacements.Add(m.replaceThis, m.byReplacement);
            replaced.Add(m.byReplacement, m.replaceThis);
        }
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
        if (changedMaterials == null) changedMaterials = new Dictionary<Renderer, int[]>();
        if (!isChanged)
        {
            UpdateMaterials();
            foreach (Renderer r in rend)
            {
                List<int> mats = new List<int>();
                Material[] sm = r.sharedMaterials;
                for (int i = 0; i < r.sharedMaterials.Length; i++)
                {
                    if (replacements.ContainsKey(r.sharedMaterials[i]))
                    {
                        sm[i] = replacements[r.sharedMaterials[i]];
                        mats.Add(i);
                    }
                }
                r.sharedMaterials = sm;

                if (mats.Count > 0)
                    changedMaterials.Add(r, mats.ToArray());
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
            foreach (KeyValuePair<Renderer, int[]> kv in changedMaterials)
            {
                Material[] sm = kv.Key.sharedMaterials;
                for (int i = 0; i < kv.Value.Length; i++)
                    sm[kv.Value[i]] = replaced[sm[kv.Value[i]]];
                kv.Key.sharedMaterials = sm;
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
