using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using uParser;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MaterialsReplacer : MonoBehaviour
{
    [SerializeField]
    int parentSearch = 0;
    [SerializeField]
    [OnValueChanged("UpdateMaterials", true)]
    MaterialPair[] materialReplacements = null;

    RendererData[] rend;
    Material[] tmpMaterials;
    Dictionary<RendMat, Material> changedMaterials;
    Dictionary<Material, Material> replacements;
    bool isChanged;

    Renderer[] instanced;

    void UpdateMaterials()
    {
        replacements = replacements.ClearOrCreate();
        foreach (MaterialPair m in materialReplacements)
            replacements.Add(m.replaceThis, m.byReplacement);
    }

    public void UpdateRenderers()
    {
        RemoveInstances();
        Transform parent = transform;
        for (int i = 0; i < parentSearch; i++)
            parent = parent.parent;
        Renderer[] r = parent.GetComponentsInChildren<Renderer>(true);
        rend = new RendererData[r.Length];
        for (int i = 0; i < rend.Length; i++)
            rend[i] = new RendererData(r[i]);
    }

    void OnEnable()
    {
        UpdateMaterials();
        UpdateRenderers();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
            Undo.postprocessModifications += OnPostprocess;
        }
        else
#endif
            ReplaceMaterials();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
            Undo.postprocessModifications -= OnPostprocess;

            EditorApplication.delayCall += () => RemoveInstances();
        }
        else
#endif
            ResetMaterials();
    }

#if UNITY_EDITOR
    void Update()
    {
        bool updateInstances = false;
        for (int i = 0; i < rend.Length; i++)
        {
            if (rend[i].IsNull())
            {
                UpdateRenderers();
                updateInstances = true;
                break;
            }
            rend[i].UpdateActiveState();
        }

        if ((!updateInstances) && (instanced.IsNullOrEmpty() || (instanced.Length != rend.Length)))
            updateInstances = true;

        if (!updateInstances)
        {
            for (int i = 0; i < instanced.Length; i++)
                if (instanced[i] == null)
                {
                    updateInstances = true;
                    break;
                }
        }

        if (updateInstances)
            Reinstantiate();
    }

    UndoPropertyModification[] OnPostprocess(UndoPropertyModification[] modifications)
    {
        bool updateInstances = false;

        foreach (UndoPropertyModification m in modifications)
        {
            if (m.currentValue.target is Renderer ren)
            {
                for (int i = 0; i < rend.Length; i++)
                    if (rend[i].rend == ren)
                    {
                        if ((!instanced.IsNullOrEmpty()) && (instanced[i] != null))
                        {
                            instanced[i].GetCopyOf(rend[i].rend);
                            ReplaceMaterials(instanced[i], false);
                        }
                        else updateInstances = true;
                    }
            }
            else if (m.currentValue.target is MeshFilter filter)
            {
                for (int i = 0; i < rend.Length; i++)
                    if (rend[i].gameObject == filter.gameObject)
                    {
                        if ((!instanced.IsNullOrEmpty()) && (instanced[i] != null))
                        {
                            MeshFilter fil = instanced[i].GetComponent<MeshFilter>();
                            fil.GetCopyOf(filter);
                        }
                        else updateInstances = true;
                    }
            }
            else if (m.currentValue.target is GameObject obj)
            {
                for (int i = 0; i < rend.Length; i++)
                    if (rend[i].gameObject == obj)
                        rend[i].UpdateActiveState();
            }
        }

        if (updateInstances)
            Reinstantiate();

        return modifications;
    }

    void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
    {
        for (int i = 0; i < rend.Length; i++)
            if (rend[i].enabled)
                rend[i].rend.enabled = false;
        //ReplaceMaterials();
    }

    void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
    {
        for (int i = 0; i < rend.Length; i++)
            if (rend[i].enabled)
                rend[i].rend.enabled = true;
        //ResetMaterials();
    }
#endif

    void ReplaceMaterials()
    {
        changedMaterials = changedMaterials.CreateIfNull();
        if (!isChanged)
        {
            foreach (RendererData r in rend)
                ReplaceMaterials(r.rend);

            isChanged = true;
        }
    }

    void ResetMaterials()
    {
        if (isChanged)
        {
            foreach (KeyValuePair<RendMat, Material> kv in changedMaterials)
                if (kv.Key.rend != null)
                {
                    Material[] sm = kv.Key.rend.sharedMaterials;
                    sm[kv.Key.mat] = kv.Value;
                    kv.Key.rend.sharedMaterials = sm;
                }
            changedMaterials.Clear();
            isChanged = false;
        }
    }

    void ReplaceMaterials(Renderer renderer, bool register = true)
    {
        tmpMaterials = renderer.sharedMaterials;
        for (int i = 0; i < tmpMaterials.Length; i++)
            if ((tmpMaterials[i] != null) &&
                replacements.TryGetValue(tmpMaterials[i], out Material replacement))
            {
                if (register)
                    changedMaterials.Add(new RendMat(renderer, i), tmpMaterials[i]);
                tmpMaterials[i] = replacement;
            }
        renderer.sharedMaterials = tmpMaterials;
    }

#if UNITY_EDITOR
    void Reinstantiate()
    {
        RemoveInstances();
        InstanceRenderersCopy();
    }

    void InstanceRenderersCopy()
    {
        instanced = new Renderer[rend.Length];
        for (int i = 0; i < rend.Length; i++)
            if (!rend[i].IsNull())
            {
                GameObject orig = rend[i].gameObject;
                GameObject go = new GameObject(orig.name + "_InstancedCopy");
                go.hideFlags = HideFlags.HideAndDontSave;
                go.transform.SetParent(orig.transform, false);
                go.layer = orig.layer;
                go.tag = orig.tag;
                go.isStatic = orig.isStatic;
                MeshFilter origFilter = orig.GetComponent<MeshFilter>();
                if (origFilter != null)
                {
                    MeshFilter filter = go.AddComponent<MeshFilter>();
                    filter.GetCopyOf(origFilter);
                }
                instanced[i] = go.AddComponent(rend[i].rend.GetType()) as Renderer;
                instanced[i].GetCopyOf(rend[i].rend);
                ReplaceMaterials(instanced[i], false);
            }
    }

    void RemoveInstances()
    {
        if (instanced != null)
            for (int i = 0; i < instanced.Length; i++)
                if (instanced[i] != null)
                    instanced[i].gameObject.DestroyOrImmediate();
        instanced = null;
    }
#endif

    [ContextMenu("Replace Permanently")]
    public void ReplacePermanently()
    {
        ReplaceMaterials();
        changedMaterials.Clear();
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

    [Serializable]
    struct RendererData
    {
        public Renderer rend;
        public GameObject gameObject;
        public bool enabled;

        public RendererData(Renderer rend)
        {
            this.rend = rend;
            gameObject = rend.gameObject;
            enabled = rend.enabled;
        }

        public bool IsNull()
        {
            return rend == null;
        }

        public bool UpdateActiveState(bool state)
        {
            bool active = state;
            if (enabled != active)
            {
                enabled = active;
                return true;
            }
            else return false;
        }

        public bool UpdateActiveState()
        {
            return UpdateActiveState(rend.enabled);
        }
    }
}
