using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class MaterialPerCameraTweaker : MonoBehaviour
{
    [HideLabel]
    [InlineProperty]
    [SerializeField]
    MaterialSource materialSource = new MaterialSource(true, -1, false);
    [SerializeField]
    bool allCameras = true;
    [SerializeField]
    [Indent]
    [ShowIf("@allCameras && materialSource.IsTypeRenderer()")]
    bool ignoreCullingMask = false;
    [SerializeField]
    [Indent]
    [HideIf("allCameras")]
    ObjectRef<Camera>[] cameras =
        new ObjectRef<Camera>[] { new ObjectRef<Camera>("Camera", "MainCamera") };
    [InfoBox(
    "This was a good idea but it apparently causes " +
        "permanent changes to materials in editor and doesn't work properly",
        infoMessageType: InfoMessageType.Error)]
    [SerializeField]
    Feature featureToModify = Feature.ArbitraryProperties;
    [SerializeField]
    [ShowIf("featureToModify", Feature.ArbitraryProperties)]
    ArbitraryProperty[] properties = null;
    [SerializeField]
    [ShowIf("featureToModify", Feature.RenderQueue)]
    int renderQueue = 3000;

    Dictionary<Material, int> oldQueues;
    List<Material> modified;

    enum Feature { ArbitraryProperties, RenderQueue }

    void OnEnable()
    {
        modified = new List<Material>();
        oldQueues = new Dictionary<Material, int>();

        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        RenderPipelineManager.endCameraRendering += EndCameraRendering;
    }

    void OnDisable()
    {
        modified.Clear();
        oldQueues.Clear();

        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= EndCameraRendering;
    }

    public void UpdateRenderers()
    {
        if (this.IsActiveAndEnabled())
            materialSource.UpdateRenderers(gameObject);
    }

    void BeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (IsCameraValid(cam))
        {
            if (materialSource.IsTypeRenderer())
            {
                RendMat[] rendMats = materialSource.GetRendMats(gameObject);
                foreach (RendMat rendMat in rendMats)
                    if (IsCameraValid(cam, rendMat.rend.gameObject))
                        Apply(rendMat.sharedMaterial);
            }
            else
            {
                Material[] mats = materialSource.GetMaterials(gameObject);
                foreach (Material mat in mats)
                    Apply(mat);
            }
        }
    }

    void EndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        Restore();
    }

    bool IsCameraValid(Camera cam, GameObject obj)
    {
        return cam.IsCameraInScene(obj.scene) &&
            (ignoreCullingMask || ((LayerMask)cam.cullingMask).ContainsLayer(obj.layer));
    }

    bool IsCameraValid(Camera cam)
    {
        return allCameras || cameras.Contains(cam);
    }

    void Apply(Material mat)
    {
        if (mat != null)
        {
            switch (featureToModify)
            {
                case Feature.ArbitraryProperties:
                    foreach (ArbitraryProperty property in properties)
                    {
                        property.SaveOriginal(mat);
                        property.SetProperty(mat);
                    }
                    break;
                case Feature.RenderQueue:
                    oldQueues = oldQueues.CreateAdd(mat, mat.renderQueue);
                    mat.renderQueue = renderQueue;
                    break;
            }
            modified.Add(mat);
        }
    }

    void Restore()
    {
        foreach (Material mat in modified)
        {
            switch (featureToModify)
            {
                case Feature.ArbitraryProperties:
                    foreach (ArbitraryProperty property in properties)
                        property.ResetProperty(mat);
                    break;
                case Feature.RenderQueue:
                    if (oldQueues.NotNullContainsKey(mat))
                    {
                        mat.renderQueue = oldQueues[mat];
                        oldQueues.Clear();
                    }
                    break;
            }
        }
    }

    public void Set(bool affectsChildren, int materialIndex, bool updateRenderers)
    {
        materialSource = new MaterialSource(affectsChildren, materialIndex, updateRenderers);
    }

    public void Set(Material[] materials)
    {
        materialSource = new MaterialSource(materials);
    }

    public void Set(Material material)
    {
        Set(new Material[] { material });
    }

    public void SetRenderQueue(int queue)
    {
        renderQueue = queue;
    }

    [Serializable]
    public struct MaterialSource
    {
        [SerializeField]
        Type sourceType;
        [SerializeField]
        [Indent]
        [ShowIf("sourceType", Type.Material)]
        Material[] materials;
        [SerializeField]
        [Indent]
        [HideLabel]
        [InlineProperty]
        [ShowIf("sourceType", Type.Renderer)]
        RendererSource rendererSource;

        List<RendMat> rendMats;
        List<Material> mats;

        public MaterialSource(Material[] materials)
        {
            sourceType = Type.Material;
            this.materials = materials;
            rendererSource = new RendererSource(true, -1, false);
            rendMats = new List<RendMat>();
            mats = null;
        }

        public MaterialSource(bool affectsChildren, int materialIndex, bool updateRenderers)
        {
            sourceType = Type.Renderer;
            materials = null;
            rendererSource = new RendererSource(affectsChildren, materialIndex, updateRenderers);
            rendMats = null;
            mats = new List<Material>();
        }

        public void UpdateRenderers(GameObject obj)
        {
            if (sourceType == Type.Renderer)
                rendererSource.UpdateRenderers(obj);
        }

        public RendMat[] GetRendMats(GameObject obj)
        {
            if (sourceType == Type.Renderer)
                return rendererSource.GetRendMats(obj);
            else
            {
                rendMats.Clear();
                for (int i = 0; i < materials.Length; i++)
                    if (materials[i] != null)
                        rendMats.Add(new RendMat(null, i));
                return rendMats.ToArray();
            }
        }

        public Material[] GetMaterials(GameObject obj)
        {
            if (sourceType == Type.Renderer)
            {
                RendMat[] rendMats = rendererSource.GetRendMats(obj);
                mats.Clear();
                for (int i = 0; i < rendMats.Length; i++)
                    mats.Add(rendMats[i].sharedMaterial);
                materials = mats.ToArray();
            }

            return materials;
        }

        public bool IsTypeRenderer()
        {
            return sourceType == Type.Renderer;
        }

        public enum Type { Renderer, Material }
    }

    [Serializable]
    public struct RendererSource
    {
        Renderer[] rend;

        [SerializeField]
        [Tooltip("Won't work on editor because it requires instancing components")]
        bool affectsChildren;
        [SerializeField]
        [Tooltip("It will be applied to all materials if set to a negative number")]
        int materialIndex;
        [SerializeField]
        bool updateRenderers;

        List<RendMat> rendMats;

        public RendererSource(bool affectsChildren, int materialIndex, bool updateRenderers)
        {
            rend = null;
            this.affectsChildren = affectsChildren;
            this.materialIndex = materialIndex;
            this.updateRenderers = updateRenderers;
            rendMats = new List<RendMat>();
        }

        void TryInitialize(GameObject obj)
        {
            if (updateRenderers || (!IsInitialized()))
                UpdateRenderers(obj);
        }

        public void UpdateRenderers(GameObject obj)
        {
            if (affectsChildren)
                rend = obj.GetComponentsInChildren<Renderer>(true);
            else
            {
                Renderer r = obj.GetComponent<Renderer>();
                if (r != null) rend = new Renderer[] { r };
            }
        }

        bool IsInitialized()
        {
            return (rend != null) && (rend.Length > 0);
        }

        public RendMat[] GetRendMats(GameObject obj)
        {
            TryInitialize(obj);

            if (IsInitialized())
            {
                rendMats.Clear();
                for (int i = 0; i < rend.Length; i++)
                {
                    Renderer r = rend[i];
                    if (r != null)
                    {
                        Material[] shM = r.sharedMaterials;
                        if (materialIndex < 0)
                            for (int j = 0; j < shM.Length; j++)
                                rendMats.Add(new RendMat(r, j));
                        else if (materialIndex < shM.Length)
                            rendMats.Add(new RendMat(r, materialIndex));
                    }
                }
                return rendMats.ToArray();
            }
            else return new RendMat[0];
        }
    }
}

