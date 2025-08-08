using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
# endif

[ExecuteAlways]
public class MaterialsReplacer : MonoBehaviour
{
    [SerializeField]
    int parentSearch = 0;
    [SerializeField]
    MaterialPair[] materialReplacements = null;
    [SerializeField]
    bool reverseReplacement = false;
#if UNITY_EDITOR
    [SerializeField]
    [OnValueChanged("Restart")]
    EditorMode editorMode = EditorMode.OnRendering;
# endif

    enum EditorMode { OnRendering, OnEnable, DontReplace}

    Renderer[] rend;
    Dictionary<RendMat, Material> changedMaterials;
    Dictionary<Material, Material> replacements;
    Dictionary<Material, Material> inverseReplacements;
    bool isChanged;

    void UpdateMaterials()
    {
        replacements = replacements.ClearOrCreate();
        inverseReplacements = inverseReplacements.ClearOrCreate();
        foreach (MaterialPair m in materialReplacements)
        {
            replacements.Add(m.replaceThis, m.byReplacement);
            inverseReplacements.Add(m.byReplacement, m.replaceThis);
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
        UpdateRenderers();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            switch (editorMode)
            {
                case EditorMode.OnRendering:
                    RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
                    RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
                    break;
                case EditorMode.OnEnable:
                    EditorSceneManager.sceneSaving += OnSceneBeingSaved;
                    EditorSceneManager.sceneSaved += OnSceneFinishedSaving;
                    ReplaceMaterials();
                    break;
                default:
                    return;
            }
        }
        else
# endif
            ReplaceMaterials();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            EditorSceneManager.sceneSaving -= OnSceneBeingSaved;
            EditorSceneManager.sceneSaved -= OnSceneFinishedSaving;
        }
#endif
        ResetMaterials();
    }

    public void Restart()
    {
        OnDisable();
        OnEnable();
    }

#if UNITY_EDITOR
    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        ReplaceMaterials();
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        ResetMaterials();
    }

    void OnSceneBeingSaved(Scene scene, string path)
    {
        ResetMaterials();
    }

    void OnSceneFinishedSaving(Scene scene)
    {
        ReplaceMaterials();
    }
#endif

    void ReplaceMaterials()
    {
        changedMaterials = changedMaterials.CreateIfNull();
        if (!isChanged)
        {
            UpdateMaterials();
            foreach (Renderer r in rend)
                if (r != null)
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

    void ResetMaterials()
    {
        if (isChanged)
        {
            if (reverseReplacement)
            {
                UpdateMaterials();
                foreach (Renderer r in rend)
                    if (r != null)
                    {
                        Material[] shM = r.sharedMaterials;
                        for (int i = 0; i < shM.Length; i++)
                            if (inverseReplacements.ContainsKey(shM[i]))
                                shM[i] = inverseReplacements[shM[i]];
                        r.sharedMaterials = shM;
                    }
            }
            else foreach (KeyValuePair<RendMat, Material> kv in changedMaterials)
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
