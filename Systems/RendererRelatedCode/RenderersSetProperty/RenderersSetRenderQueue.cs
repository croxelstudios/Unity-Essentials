using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RenderersSetRenderQueue : MonoBehaviour
{
    Renderer[] rend;

    [SerializeField]
    [Tooltip("Won't work on editor because it requires instancing components")]
    bool affectsChildren = true;
    [SerializeField]
    [Tooltip("It will be applied to all materials if set to a negative number")]
    int materialIndex = -1;
    [SerializeField]
    bool updateRenderers = false;
    [SerializeField]
    int renderQueue = 3000;

    Dictionary<Material, RenderQueueData> oldQueues;
    Dictionary<GameObject, RenderersSetRenderQueue> dicRSRQ;
    static List<GameObject> dominated;

    void OnEnable()
    {
        if (dominated == null) dominated = new List<GameObject>();
        if (!dominated.Contains(gameObject))
            UpdateRenderersInternal();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            if (affectsChildren)
            {
                if (dicRSRQ != null)
                    foreach (KeyValuePair<GameObject, RenderersSetRenderQueue> pair in dicRSRQ)
                    {
                        pair.Value.enabled = false;
                        pair.Value.UpdateRenderers();
                    }
            }
    }

    void OnDestroy()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            if (affectsChildren)
            {
                KeyValuePair<GameObject, RenderersSetRenderQueue>[] pairs = 
                    new KeyValuePair<GameObject, RenderersSetRenderQueue>[dicRSRQ.Count];
                int ind = 0;
                if (dicRSRQ != null)
                    foreach (KeyValuePair<GameObject, RenderersSetRenderQueue> pair in dicRSRQ)
                    {
                        pairs[ind] = pair;
                        ind++;
                    }
                for (int i = 0; i < pairs.Length; i++)
                {
                    dominated.Remove(pairs[i].Key);
                    Destroy(pairs[i].Value);
                }
                dicRSRQ.Clear();
            }
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

#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            if (affectsChildren)
            {
                if (dicRSRQ == null) dicRSRQ = new Dictionary<GameObject, RenderersSetRenderQueue>();
                for (int i = 0; i < rend.Length; i++)
                {
                    if (dicRSRQ.ContainsKey(rend[i].gameObject))
                        dicRSRQ[rend[i].gameObject].enabled = true;
                    else if (rend[i].gameObject != gameObject)
                    {
                        dominated.Add(rend[i].gameObject);
                        RenderersSetRenderQueue rsrq = rend[i].gameObject.AddComponent<RenderersSetRenderQueue>();
                        rsrq.Set(false, materialIndex, false);
                        rsrq.SetRenderQueue(renderQueue);
                        rsrq.UpdateRenderers();
                        dicRSRQ.Add(rend[i].gameObject, rsrq);
                    }
                }
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

#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            if (affectsChildren)
            {
                if (dicRSRQ != null)
                    foreach (KeyValuePair<GameObject, RenderersSetRenderQueue> pair in dicRSRQ)
                        pair.Value.SetRenderQueue(queue);
            }
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
