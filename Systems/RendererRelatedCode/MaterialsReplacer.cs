using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[DefaultExecutionOrder(-1)]
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
    MeshFilter[] instancedFilters;
    bool updateInstances = false;
    bool additionWasTracked = false;
    List<RendererData> tmpList;

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
        tmpList = tmpList.CreateIfNull();
        for (int i = 0; i < r.Length; i++)
            if (!r[i].gameObject.hideFlags.Contains(HideFlags.HideAndDontSave))
                tmpList.Add(new RendererData(r[i]));
        rend = tmpList.ToArray();
        tmpList.Clear();
    }

    void OnEnable()
    {
        UpdateMaterials();
        UpdateRenderers();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            updateInstances = true;
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            Undo.postprocessModifications += OnPostprocess;
            SceneView.duringSceneGui += OnSceneGUI;
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
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            Undo.postprocessModifications -= OnPostprocess;
            SceneView.duringSceneGui -= OnSceneGUI;

            EditorApplication.delayCall += () => RemoveInstances();
        }
        else
#endif
            ResetMaterials();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (!Application.isPlaying)
        {
            if (!updateInstances)
                for (int i = 0; i < rend.Length; i++)
                {
                    if (rend[i].IsNull())
                    {
                        UpdateRenderers();
                        updateInstances = true;
                        break;
                    }
                    else if (rend[i].UpdateActiveState())
                        updateInstances = true;
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
    }
    void OnSceneGUI(SceneView sv)
    {
        if (!additionWasTracked)
        {
            Event e = Event.current;
            if (e.type == EventType.DragPerform)
            {
                updateInstances = true;
                additionWasTracked = true;
            }
        }
    }

    UndoPropertyModification[] OnPostprocess(UndoPropertyModification[] modifications)
    {
        foreach (UndoPropertyModification m in modifications)
        {
            PropertyModification pm = m.currentValue;
            if (pm.target is Renderer ren)
            {
                for (int i = 0; i < rend.Length; i++)
                    if (rend[i].rend == ren)
                    {
                        if (pm.propertyPath == "m_Enabled")
                        {
                            //rend[i].UpdateActiveState(pm.value.Parse<bool>());
                            //updateInstances = true;
                        }
                        else if ((!instanced.IsNullOrEmpty()) && (instanced[i] != null))
                        {
                            instanced[i].GetCopyOf(ren);
                            ReplaceMaterials(instanced[i], false);
                        }
                        else updateInstances = true;
                    }
            }
            else if (pm.target is MeshFilter filter)
            {
                for (int i = 0; i < rend.Length; i++)
                    if (rend[i].filter == filter)
                    {
                        if ((!instancedFilters.IsNullOrEmpty()) && (instancedFilters[i] != null))
                            instancedFilters[i].GetCopyOf(filter);
                        else updateInstances = true;
                    }
            }
            else if (pm.target is Transform tr)
            {
                for (int i = 0; i < rend.Length; i++)
                    if (rend[i].transform.IsChildOf(tr))
                    {
                        if ((!instanced.IsNullOrEmpty()) && (instanced[i] != null))
                        {
                            instanced[i].GetCopyOf(rend[i].rend);
                            ReplaceMaterials(instanced[i], false);
                        }
                        else updateInstances = true;
                    }
            }
        }

        return modifications;
    }

    void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
    {
        if (rend != null)
            for (int i = 0; i < rend.Length; i++)
                if (rend[i].enabled)
                    rend[i].rend.enabled = false;
        //ReplaceMaterials();
    }

    void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
    {
        if (rend != null)
            for (int i = 0; i < rend.Length; i++)
                if (rend[i].enabled)
                    rend[i].rend.enabled = true;
        //ResetMaterials();
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (instancedFilters != null)
            for (int i = 0; i < instancedFilters.Length; i++)
                if (instancedFilters[i] != null)
                    instancedFilters[i].sharedMesh = rend[i].filter.sharedMesh;
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
        updateInstances = false;
    }

    void InstanceRenderersCopy()
    {
        instanced = new Renderer[rend.Length];
        instancedFilters = new MeshFilter[rend.Length];
        for (int i = 0; i < rend.Length; i++)
            if ((!rend[i].IsNull()) && rend[i].enabled)
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
                    instancedFilters[i] = go.AddComponent<MeshFilter>();
                    instancedFilters[i].GetCopyOf(origFilter);
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
        public MeshFilter filter;
        public Renderer rend;
        public GameObject gameObject;
        public Transform transform;
        public bool enabled;

        public RendererData(Renderer rend)
        {
            filter = rend.GetComponent<MeshFilter>();
            this.rend = rend;
            gameObject = rend.gameObject;
            transform = gameObject.transform;
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
