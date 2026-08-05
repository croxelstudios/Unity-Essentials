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
    Dictionary<RendMat, Material> originalMaterials;
    Dictionary<RendMat, Material> changedMaterials;
    Dictionary<Material, Material> replacements;
    bool isChanged;

    Renderizable[] instanced;
    MeshFilter[] instancedFilters;
    List<RendererData> tmpList;
    bool preLoaded;
#if UNITY_EDITOR
    bool updateInstances = false;
    bool additionWasTracked = false;
#endif

    Transform parent;

    void UpdateMaterials()
    {
        replacements = replacements.ClearOrCreate();
        foreach (MaterialPair m in materialReplacements)
            replacements.Add(m.replaceThis, m.byReplacement);
    }

    public void UpdateRenderers()
    {
#if UNITY_EDITOR
        RemoveInstances();
#endif
        parent = transform;
        for (int i = 0; i < parentSearch; i++)
            parent = parent.parent;
        Renderizable[] r = Renderizable.GetAll(parent, Scope.inChildren, true);
        tmpList = tmpList.CreateIfNull();
        for (int i = 0; i < r.Length; i++)
            if (!r[i].gameObject.hideFlags.Contains(HideFlags.HideAndDontSave))
                tmpList.Add(new RendererData(r[i]));
        rend = tmpList.ToArray();
        tmpList.Clear();
    }

    void OnEnable()
    {
        if (!preLoaded)
        {
            UpdateMaterials();
            UpdateRenderers();
        }
        else preLoaded = false;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            updateInstances = true;
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            OnEditorChange.PropertyModification_In(PropertyModification);
            SceneView.duringSceneGui += OnSceneGUI;
        }
        else
#endif
        {
            if (!isChanged)
                ReplaceMaterials();
        }
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            OnEditorChange.PropertyModification_Out(PropertyModification);
            SceneView.duringSceneGui -= OnSceneGUI;

            EditorApplication.delayCall += () => RemoveInstances();
        }
        else
#endif
        {
            if (parent.gameObject.activeInHierarchy)
                TryDisable();
            else ActivationTracker.TrackActivation(parent, TryDisable);
        }
    }

    void OnDestroy()
    {
        if (isChanged)
            ResetMaterials();
    }

    void TryDisable()
    {
        if (isChanged && (!enabled))
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

    void PropertyModification(PropertyModification pm)
    {
        if (pm.target is Renderer ren)
        {
            for (int i = 0; i < rend.Length; i++)
                if (rend[i].renderer == ren)
                {
                    if (pm.propertyPath == "m_Enabled")
                    {
                        //rend[i].UpdateActiveState(pm.value.Parse<bool>());
                        //updateInstances = true;
                    }
                    else if ((!instanced.IsNullOrEmpty()) && (!instanced[i].IsNull()))
                    {
                        instanced[i].renderer.GetCopyOf(ren);
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
                    if ((!instanced.IsNullOrEmpty()) && (!instanced[i].IsNull()))
                    {
                        instanced[i].renderer.GetCopyOf(rend[i].renderer);
                        ReplaceMaterials(instanced[i], false);
                    }
                    else updateInstances = true;
                }
        }
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
                if (!kv.Key.rend.IsNull())
                {
                    Material[] sm = kv.Key.rend.sharedMaterials;
                    sm[kv.Key.mat] = kv.Value;
                    kv.Key.rend.sharedMaterials = sm;
                }
            changedMaterials.Clear();
            isChanged = false;
        }
    }

    void ReplaceMaterials(Renderizable renderizable, bool register = true)
    {
        tmpMaterials = renderizable.sharedMaterials;
        for (int i = 0; i < tmpMaterials.Length; i++)
        {
            RendMat rendMat = new RendMat(renderizable, i);

            if (tmpMaterials[i].name.Contains(" (Instance)") &&
                originalMaterials.SmartGetValue(rendMat, out Material m))
                tmpMaterials[i] = m;

            if ((tmpMaterials[i] != null) &&
                replacements.TryGetValue(tmpMaterials[i], out Material replacement))
            {
                if (register)
                {
                    changedMaterials.Add(rendMat, tmpMaterials[i]);
                    if (!originalMaterials.NotNullContainsKey(rendMat))
                        originalMaterials = originalMaterials.CreateAdd(rendMat, tmpMaterials[i]);
                }
                tmpMaterials[i] = replacement;
            }
        }
        renderizable.sharedMaterials = tmpMaterials;
    }

    public void Load()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            if (!this.IsActiveAndEnabled())
                preLoaded = true;
            UpdateMaterials();
            UpdateRenderers();
        }
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
        instanced = new Renderizable[rend.Length];
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
                    instancedFilters[i] = go.AddComponentCopy(origFilter);
                instanced[i] = new Renderizable(go.AddComponentCopy(rend[i].rend.renderer));
                ReplaceMaterials(instanced[i], false);
            }
    }

    void RemoveInstances()
    {
        if (instanced != null)
            for (int i = 0; i < instanced.Length; i++)
                if (!instanced[i].IsNull())
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
        public Renderizable rend;
        public MeshFilter filter { get { return rend.filter; } }
        public Renderer renderer { get { return rend.renderer; } }
        public CustomRenderer customRenderer { get { return rend.customRenderer; } }
        public GameObject gameObject { get { return rend.gameObject; } }
        public Transform transform { get { return rend.transform; } }
        public bool enabled;

        public RendererData(Renderizable rend)
        {
            this.rend = rend;
            enabled = rend.enabled;
        }

        public bool IsNull()
        {
            return rend.IsNull();
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
