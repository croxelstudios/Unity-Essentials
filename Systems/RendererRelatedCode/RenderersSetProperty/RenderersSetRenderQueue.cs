using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEditor;

[ExecuteAlways]
public class BRenderersSetRenderQueue : MonoBehaviour
{
    Renderer[] rend;

    [SerializeField]
    bool affectsChildren = true;
    [SerializeField]
    [Tooltip("It will be applied to all materials if set to a negative number")]
    int materialIndex = -1;
    [SerializeField]
    bool updateRenderers = false;
    [SerializeField]
    int renderQueue = 3000;

    Dictionary<Material, RenderQueueData> oldQueues = new Dictionary<Material, RenderQueueData>();

    void OnEnable()
    {
        UpdateRenderersInternal();
    }

    bool IsInitialized()
    {
        return (rend != null) && (rend.Length > 0);
    }

    void UpdateRenderersInternal()
    {
        if (affectsChildren)
            rend = GetComponentsInChildren<Renderer>(true);
        else
        {
            Renderer r = GetComponent<Renderer>();
            if (r != null) rend = new Renderer[] { r };
        }
    }

    public void UpdateRenderers()
    {
        if (this.IsActiveAndEnabled())
            UpdateRenderersInternal();
    }

    void OnWillRenderObject()
    {
        ApplyRenderQueue();
    }

    void OnRenderObject()
    {
        RestoreRenderQueue();
    }

    void ApplyRenderQueue()
    {
        if (updateRenderers)
            UpdateRenderersInternal();

        if (IsInitialized())
            for (int i = 0; i < rend.Length; i++)
                if (rend[i] != null)
                {
                    if (materialIndex < 0)
                        for (int j = 0; j < rend[i].sharedMaterials.Length; j++)
                            ApplyRenderQueue(i, j);
                    else if (materialIndex < rend[i].sharedMaterials.Length)
                        ApplyRenderQueue(i, materialIndex);
                }
    }

    void ApplyRenderQueue(int rendId, int materialId)
    {
        Material mat = rend[rendId].sharedMaterials[materialId];
        if (mat != null)
        {
            bool wasOverriden = false;
            //TO DO: Checking if the value is overriden seems imposible
            if (oldQueues == null) oldQueues = new Dictionary<Material, RenderQueueData>();
            if (!oldQueues.ContainsKey(mat))
                oldQueues.Add(mat, new RenderQueueData(mat.renderQueue, wasOverriden));
            mat.renderQueue = renderQueue;
        }
    }

    void RestoreRenderQueue()
    {
        if (oldQueues != null)
        {
            foreach (KeyValuePair<Material, RenderQueueData> queue in oldQueues)
            {
                queue.Key.renderQueue = queue.Value.renderQueue;
                //TO DO: Changing if the value is overriden seems imposible
            }
            oldQueues.Clear();
        }
    }

    public void Set(bool affectsChildren, int materialIndex, bool updateRenderers)
    {
        this.affectsChildren = affectsChildren;
        this.materialIndex = materialIndex;
        this.updateRenderers = updateRenderers;
    }

    public void SetRenderQueue(int queue)
    {
        renderQueue = queue;
    }

    struct RenderQueueData
    {
        public int renderQueue;
        public bool wasOverriden;

        public RenderQueueData(int renderQueue, bool wasOverriden)
        {
            this.renderQueue = renderQueue;
            this.wasOverriden = wasOverriden;
        }
    }
}
