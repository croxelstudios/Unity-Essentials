using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class BRendererDuplicator : MonoBehaviour
{
    [SerializeField]
    bool waitOneFrameForInit = false;
    [SerializeField]
    Component[] extraDuplicableComponents = null;

    //TO DO: duplicateChildren bool variable
    static bool enabling;
    RendererDuplicator_AfterLate afterLate;
    UniqueIntList hierarchyIds;

    const int DECIMALIDPRECISION = 2;

    #region Unity Actions
    protected virtual void Awake()
    {
        if (afterLate == null)
            afterLate = gameObject.AddComponent<RendererDuplicator_AfterLate>();
        afterLate.original = this;
        afterLate.enabled = false;
    }

    void OnEnable()
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

    void Init()
    {
        afterLate.enabled = true;
        if (!enabling)
        {
            enabling = true;
            EnableActions();
            enabling = false;
        }
    }

    protected virtual void EnableActions()
    {
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (!enabling) DisableActions();
        afterLate.enabled = false;
    }

    protected virtual void DisableActions()
    {
    }

    public virtual void UpdateEvent()
    {
    }
    #endregion

    #region Duplicates creation
    protected virtual void RecreateDuplicate(RenderingAgent source, ref RenderingAgent duplicate)
    {
        Transform parent = duplicate.parent.parent;
        Destroy(duplicate.parent.gameObject);
        duplicate = CreateDuplicate(source, parent);
    }

    protected RenderingAgent CreateDuplicate(RenderingAgent source, Transform parent)
    {
        hierarchyIds = new UniqueIntList(new int[source.largestHierarchy], 0);

        if (source.renderers.Length <= 0) return null;
        RenderingAgent duplicate = new RenderingAgent(new GameObject(source.name));

        Transform duParent = new GameObject("Renderer duplicate of " + source.name).transform;
        duParent.SetParent(parent, false);

        duParent.gameObject.SetActive(false);

        duplicate.parent = duParent;
        //Main transform setup
        duplicate.transform.position = source.transform.position;
        duplicate.transform.rotation = source.transform.rotation;
        duplicate.transform.localScale = source.transform.lossyScale;
        //

        //Structure replication
        AddComponentsToDuplicate(ref source, ref duplicate, source.transform, duplicate.gameObject, extraDuplicableComponents);
        foreach (KeyValuePair<UniqueIntList, Transform> keyValue in source.trEquivalence)
        {
            List<int> intList = new List<int>();
            Transform current = duplicate.transform;
            int[] key = keyValue.Key.ToIntArray();
            for (int i = 0; i < key.Length; i++)
            {
                for (int j = current.childCount; j <= key[i]; j++)
                {
                    GameObject child = new GameObject();
                    child.transform.parent = current;
                }
                current = current.GetChild(key[i]);
            }
            current.name = keyValue.Value.name;
            AddComponentsToDuplicate(ref source, ref duplicate, keyValue.Value, current.gameObject, extraDuplicableComponents);
        }
        //

        UpdateSkinnedMeshes(source, ref duplicate);

        CopyRenderersEnabledState(source.renderers, duplicate.renderers);
        CopyChildsActiveState(source, duplicate);

        duParent.gameObject.SetActive(true);
        return duplicate;
    }

    protected RenderingAgent[] CreateDuplicates(RenderingAgent source, int amount, Transform parent)
    {
        if (source.renderers.Length <= 0) return null;
        RenderingAgent[] duplicates = new RenderingAgent[amount];

        for (int i = 0; i < duplicates.Length; i++)
            duplicates[i] = CreateDuplicate(source, parent);

        return duplicates;
    }

    protected void ReplaceRendererData(RenderingAgent duplicate, Material[] replaceMaterials,
        int queueAdd = 0, int sortOrderAdd = 0, bool replaceAllMaterials = false,
        string replaceLayer = "", int replaceSortingLayer = -1)
    {
        foreach (Renderer rend in duplicate.renderers)
        {
            if (!string.IsNullOrEmpty(replaceLayer))
                rend.gameObject.layer = LayerMask.NameToLayer(replaceLayer);
            if (replaceAllMaterials) rend.sharedMaterials = replaceMaterials;
            else OverrideMaterials(rend, replaceMaterials);
            if (queueAdd != 0)
                for (int j = 0; j < rend.sharedMaterials.Length; j++)
                {
                    rend.materials[j].renderQueue += queueAdd;
                    //TO DO: Desplegable con opciones de a qué material aplicárselo
                }
            rend.sortingOrder += sortOrderAdd;
        }
    }

    protected void ReplaceRenderersData(RenderingAgent[] duplicates, Material[] replaceMaterials,
        int queueMultiplier = 0, int sortOrderMultiplier = 0, bool replaceAllMaterials = false,
        string replaceLayer = "", int replaceSortingLayer = -1)
    {
        for (int i = 0; i < duplicates.Length; i++)
        {
            ReplaceRendererData(duplicates[i], replaceMaterials, (i + 1) * queueMultiplier,
                (i + 1) * sortOrderMultiplier, replaceAllMaterials, replaceLayer, replaceSortingLayer);
        }
    }

    void AddComponentsToDuplicate(ref RenderingAgent source, ref RenderingAgent duplicate, Transform child,
        GameObject target, Component[] extraDuplicableComponents = null)
    {
        Renderer rend = source.RendererOfTransform(child);
        if (rend != null)
        {
            MeshFilter mf = rend.GetComponent<MeshFilter>();
            if (mf != null) target.AddComponentCopy(mf);

            if (extraDuplicableComponents != null)
                for (int i = 0; i < extraDuplicableComponents.Length; i++)
                {
                    Component c = extraDuplicableComponents[i];
                    if (c.gameObject == child.gameObject)
                        target.AddComponentCopy(c);
                }

            Renderer copy = target.AddComponentCopy(rend);
            copy.sortingLayerID = rend.sortingLayerID;
            copy.sortingOrder = rend.sortingOrder;
        }

        BRenderersSetProperty[] rsp;
        if (source.setProperties.ContainsKey(child))
            rsp = source.setProperties[child];
        else
        {
            rsp = source.gameObject.GetComponents<BRenderersSetProperty>();
            source.setProperties.Add(child, rsp);
        }

        duplicate.setProperties = duplicate.setProperties.CreateAdd(
            target.transform, new BRenderersSetProperty[rsp.Length]);
        for (int i = 0; i < source.setProperties[child].Length; i++)
            duplicate.setProperties[target.transform][i] =
                target.AddComponentCopy(source.setProperties[child][i], true);
    }

    void UpdateSkinnedMeshes(RenderingAgent source, ref RenderingAgent duplicate)
    {
        for (int i = 0; i < duplicate.skinnedRends.Length; i++)
        {
            UniqueIntList id = source.trInverseEquivalence[source.skinnedRends[i].rootBone];

            Transform current = duplicate.transform;
            int[] key = id.ToIntArray();
            for (int j = 0; j < key.Length; j++)
                current = current.GetChild(key[j]);

            duplicate.skinnedRends[i].rootBone = current;
            duplicate.skinnedRends[i].PopulateBoneArray(source.skinnedRends[i].rootBone, source.skinnedRends[i].bones);
        }
    }

    void OverrideMaterials(Renderer rend, Material[] materials)
    {
        Material[] newMaterialsArray = new Material[rend.sharedMaterials.Length];
        for (int j = 0; j < newMaterialsArray.Length; j++)
        {
            if ((j < materials.Length) && (materials[j] != null))
                newMaterialsArray[j] = materials[j];
            else if (j < rend.sharedMaterials.Length)
                newMaterialsArray[j] = rend.sharedMaterials[j];
            else newMaterialsArray[j] = null;
        }
        rend.sharedMaterials = newMaterialsArray;
    }

    Renderer[] DestroyNonRenderingComponents(GameObject obj)
    {
        List<Renderer> renderers = new List<Renderer>();
        Component[] components = obj.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            Type type = components[i].GetType();
            if (typeof(Renderer).IsAssignableFrom(type))
                renderers.Add((Renderer)components[i]);
            else if ((!typeof(MeshFilter).IsAssignableFrom(type)) &&
                (!typeof(Transform).IsAssignableFrom(type)) &&
                (!typeof(BRenderersSetProperty).IsAssignableFrom(type)))
                Destroy(components[i]);
        }
        foreach (Transform child in obj.transform)
            renderers.AddRange(DestroyNonRenderingComponents(child.gameObject));
        return renderers.ToArray();
    }
    #endregion

    #region Update Methods
    protected void UpdateDuplicate(RenderingAgent source, ref RenderingAgent duplicate)
    {
        if (source.renderers.Length != duplicate.renderers.Length)
            RecreateDuplicate(source, ref duplicate);

        CopyRenderersEnabledState(source.renderers, duplicate.renderers);
        //TO DO: Optimize this by adding components to all children of origin to track changes.
        //Maybe have a struct with info on changes?
        CopyChildsActiveState(source, duplicate);
        CopyRSPActiveState(source, duplicate);
        CopyChildTransforms(source, duplicate.transform);
        CopyRenderersData(source.renderers, duplicate.renderers);
        UpdateSkinnedMeshes(source, ref duplicate);
    }

    protected void UpdateDuplicates(RenderingAgent source, ref RenderingAgent[] duplicates)
    {
        for (int i = 0; i < duplicates.Length; i++)
            UpdateDuplicate(source, ref duplicates[i]);
    }

    void CopyRSPActiveState(RenderingAgent source, RenderingAgent target, Transform duplicate, ref UniqueIntList id)
    {
        id.Add(0);
        for (int i = 0; i < duplicate.childCount; i++)
        {
            Transform child = duplicate.GetChild(i);

            id.ReplaceLast(i);
            CopyRSPActiveState(source, target, child, ref id);

            Transform from = source.trEquivalence[id];
            for (int j = 0; j < source.setProperties[from].Length; j++)
                target.setProperties[child][j].enabled = source.setProperties[from][j].enabled;
        }
        id.RemoveLast();
    }

    void CopyRSPActiveState(RenderingAgent source, RenderingAgent target)
    {
        hierarchyIds.Clear();
        CopyRSPActiveState(source, target, target.transform, ref hierarchyIds);
        for (int j = 0; j < source.setProperties[source.transform].Length; j++)
            target.setProperties[target.transform][j].enabled = source.setProperties[source.transform][j].enabled;
    }

    void CopyChildsActiveState(RenderingAgent source, Transform duplicate, ref UniqueIntList id)
    {
        id.Add(0);
        for (int i = 0; i < duplicate.childCount; i++)
        {
            Transform child = duplicate.GetChild(i);

            id.ReplaceLast(i);
            CopyChildsActiveState(source, child, ref id);

            Transform from = source.trEquivalence[id];
            child.gameObject.SetActive(from.gameObject.activeSelf);
        }
        id.RemoveLast();
    }

    void CopyChildsActiveState(RenderingAgent source, RenderingAgent target)
    {
        hierarchyIds.Clear();
        CopyChildsActiveState(source, target.transform, ref hierarchyIds);
    }

    void CopyRenderersEnabledState(Renderer[] source, Renderer[] target)
    {
        for (int i = 0; i < source.Length; i++)
            target[i].enabled = source[i].enabled;
    }

    void CopyRendererData(Renderer source, Renderer target)
    {
        target.GetCopyOf(source);
    }

    void CopyRenderersData(Renderer[] source, Renderer[] target)
    {
        for (int i = 0; i < source.Length; i++)
            CopyRendererData(source[i], target[i]);
    }

    void CopyChildTransforms(RenderingAgent source, Transform duplicate, ref UniqueIntList id)
    {
        id.Add(0);
        for (int i = 0; i < duplicate.transform.childCount; i++)
        {
            Transform child = duplicate.GetChild(i);

            id.ReplaceLast(i);
            CopyChildTransforms(source, child, ref id);

            Transform from = source.trEquivalence[id];
            child.localPosition = from.localPosition;
            child.localRotation = from.localRotation;
            child.localScale = from.localScale;
        }
        id.RemoveLast();
    }

    void CopyChildTransforms(RenderingAgent source, Transform duplicate)
    {
        hierarchyIds.Clear();
        CopyChildTransforms(source, duplicate, ref hierarchyIds);
    }
    #endregion

    #region Useful for Inheritance
    protected void UpdateDuplicateOffset(Transform source, RenderingAgent duplicate, bool offsetLocally,
        TransformData tranformOffsetMultipliers = new TransformData())
    {
        if (duplicate.gameObject.activeInHierarchy)
        {
            Transform tr = duplicate.transform;
            if (offsetLocally)
            {
                Transform lastParent = duplicate.parent;
                tr.SetParent(source);
                tr.localPosition = tranformOffsetMultipliers.position;
                tr.localRotation = Quaternion.Euler(tranformOffsetMultipliers.eulerAngles);
                tr.localScale = Vector3.one + tranformOffsetMultipliers.localScale;
                tr.SetParent(lastParent, true);
            }
            else
            {
                tr.position = source.position + (tranformOffsetMultipliers.position);
                tr.rotation = Quaternion.Euler(tranformOffsetMultipliers.eulerAngles) * source.rotation;
                tr.localScale = source.lossyScale + (tranformOffsetMultipliers.localScale);
            }

            if (tranformOffsetMultipliers.parent != null)
            {
                Transform p = tranformOffsetMultipliers.parent;
                tr.position += p.position - source.position;
                tr.rotation = p.rotation * Quaternion.Inverse(source.rotation) * tr.rotation;
                Vector3 newScale = tr.localScale;
                if (tr.IsChildOf(p.parent)) newScale.Scale(p.localScale);
                else newScale.Scale(p.lossyScale);
                tr.localScale = newScale;
            }
        }
    }

    protected void UpdateDuplicateOffsets(Transform source, RenderingAgent[] duplicates, bool offsetLocally,
        TransformData tranformOffsetMultipliers = new TransformData())
    {
        for (int i = 0; i < duplicates.Length; i++)
        {
            TransformData tr = new TransformData();
            tr.position = (i + 1) * tranformOffsetMultipliers.position;
            tr.rotation = Quaternion.Euler((i + 1) * tranformOffsetMultipliers.eulerAngles);
            tr.localScale = ((i + 1) * tranformOffsetMultipliers.localScale);
            tr.parent = tranformOffsetMultipliers.parent;
            UpdateDuplicateOffset(source, duplicates[i], offsetLocally, tr);
        }
    }

    protected int GetDuplicateIndex(RenderingAgent[] duplicates, RenderingAgent duplicate)
    {
        for (int i = 0; i < duplicates.Length; i++)
            if (duplicates[i].gameObject == duplicate.gameObject)
                return i;
        return -1;
    }

    protected void ReplaceSortingLayer(Renderer renderer, string sortingLayer)
    {
        if (!string.IsNullOrEmpty(sortingLayer))
        {
            int id = SortingLayer.NameToID(sortingLayer);
            if (SortingLayer.IsValid(id))
            {
                int prevL = SortingLayer.GetLayerValueFromID(renderer.sortingLayerID);
                renderer.sortingLayerID = id;
                renderer.sortingOrder = (prevL * 100) + renderer.sortingOrder;
            }
        }
    }

    protected void ReplaceSortingLayers(Renderer[] renderer, string sortingLayer)
    {
        for (int i = 0; i < renderer.Length; i++)
            ReplaceSortingLayer(renderer[i], sortingLayer);
    }

    protected void ReplaceSortingLayers(RenderingAgent[] agents, string sortingLayer)
    {
        for (int i = 0; i < agents.Length; i++)
            ReplaceSortingLayers(agents[i].renderers, sortingLayer);
    }

    protected bool IsSortingLayerValid(string sortingLayer)
    {
        return (!string.IsNullOrEmpty(sortingLayer)) &&
            SortingLayer.IsValid(SortingLayer.NameToID(sortingLayer));
    }
    #endregion

    protected class RenderingAgent
    {
        public GameObject gameObject;
        Renderer[] _renderers;
        public Renderer[] renderers
        {
            get
            {
                if (_renderers == null)
                    _renderers = GetAllRenderers(transform);
                return _renderers;
            }

            set { _renderers = value; }
        }
        public string name { get { return gameObject?.name; } set { gameObject.name = value; } }
        public Transform transform { get { return gameObject?.transform; } }
        public Transform parent { get { return gameObject?.transform?.parent; } set { transform.parent = value; } }
        Dictionary<UniqueIntList, Transform> _trEquivalence;
        public Dictionary<UniqueIntList, Transform> trEquivalence
        {
            get
            {
                if (_trEquivalence == null)
                {
                    _trEquivalence = new Dictionary<UniqueIntList, Transform>();
                    _trInverseEquivalence = new Dictionary<Transform, UniqueIntList>();
                    RecursiveEquivalence(transform, ref _trEquivalence, ref _trInverseEquivalence);
                }
                return _trEquivalence;
            }
        }
        Dictionary<Transform, UniqueIntList> _trInverseEquivalence;
        public Dictionary<Transform, UniqueIntList> trInverseEquivalence
        {
            get
            {
                if (_trInverseEquivalence == null)
                {
                    _trEquivalence = new Dictionary<UniqueIntList, Transform>();
                    _trInverseEquivalence = new Dictionary<Transform, UniqueIntList>();
                    RecursiveEquivalence(transform, ref _trEquivalence, ref _trInverseEquivalence);
                }
                return _trInverseEquivalence;
            }
        }
        public Dictionary<Transform, BRenderersSetProperty[]> setProperties;
        SkinnedMeshRenderer[] _skinnedRends;
        public SkinnedMeshRenderer[] skinnedRends
        {
            get
            {
                if (_skinnedRends == null)
                    _skinnedRends = gameObject?.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                return _skinnedRends;
            }
        }
        int _largestHierarchy;
        public int largestHierarchy
        {
            get
            {
                if (_trInverseEquivalence == null)
                {
                    _trEquivalence = new Dictionary<UniqueIntList, Transform>();
                    _trInverseEquivalence = new Dictionary<Transform, UniqueIntList>();
                    RecursiveEquivalence(transform, ref _trEquivalence, ref _trInverseEquivalence);
                }
                return _largestHierarchy;
            }
        }

        Transform[] GetChildrenRecursive(Transform tr)
        {
            if (tr.childCount <= 0) return new Transform[] { };
            else
            {
                List<Transform> result = new List<Transform>();
                for (int i = 0; i < tr.childCount; i++)
                {
                    Transform child = tr.GetChild(i);
                    result.Add(child);
                    result.AddRange(GetChildrenRecursive(child));
                }
                return result.ToArray();
            }
        }

        void RecursiveEquivalence(Transform tr, ref Dictionary<UniqueIntList, Transform> equiv,
            ref Dictionary<Transform, UniqueIntList> equivInv)
        {
            List<int> list = new List<int>();
            List<Transform> skinnedRendsTr = new List<Transform>();
            for (int i = 0; i < skinnedRends.Length; i++)
            {
                Transform root = skinnedRends[i].rootBone;
                skinnedRendsTr.Add(root);
                skinnedRendsTr.AddRange(GetChildrenRecursive(root));
            }
            RecursiveEquivalence(tr, ref list, ref equiv, ref equivInv, skinnedRendsTr.ToArray());
        }

        bool RecursiveEquivalence(Transform tr, ref List<int> list, ref Dictionary<UniqueIntList, Transform> equiv,
            ref Dictionary<Transform, UniqueIntList> equivInv, params Transform[] includeForSure)
        {
            if (tr.childCount <= 0)
            {
                if (RendererOfTransform(tr) || includeForSure.Contains(tr)) return true;
                else return false;
            }
            else
            {
                list.Add(0);
                bool hasRenderer = false;
                for (int i = 0; i < tr.childCount; i++)
                {
                    Transform child = tr.GetChild(i);
                    if (RecursiveEquivalence(child, ref list, ref equiv, ref equivInv, includeForSure))
                    {
                        if (_largestHierarchy < list.Count)
                            _largestHierarchy = list.Count;
                        UniqueIntList id = new UniqueIntList(list.ToArray(), list.Count);
                        if (!equiv.ContainsKey(id))
                            equiv.Add(id, child);
                        if (!equivInv.ContainsKey(child))
                            equivInv.Add(child, id);
                        hasRenderer = true;
                        list[list.Count - 1]++;
                    }
                }
                list.RemoveAt(list.Count - 1);

                if (hasRenderer || RendererOfTransform(tr) || includeForSure.Contains(tr)) return true;
                else return false;
            }
        }

        public Renderer RendererOfTransform(Transform tr)
        {
            foreach (Renderer r in renderers)
                if (tr == r.transform)
                    return r;
            return null;
        }

        public RenderingAgent(GameObject gameObject)
        {
            this.gameObject = gameObject;
            _renderers = null;
            _trEquivalence = null;
            _trInverseEquivalence = null;
            _skinnedRends = null;
            setProperties = new Dictionary<Transform, BRenderersSetProperty[]>();
        }

        public RenderingAgent(GameObject gameObject, Renderer[] renderers)
        {
            this.gameObject = gameObject;
            _renderers = renderers;
            _trEquivalence = null;
            _trInverseEquivalence = null;
            _skinnedRends = null;
            setProperties = new Dictionary<Transform, BRenderersSetProperty[]>();
        }

        Renderer[] GetAllRenderers(Transform source)
        {
            List<Renderer> renderers = new List<Renderer>();
            Component[] components = source.gameObject?.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
                if (typeof(Renderer).IsAssignableFrom(components[i].GetType()))
                    renderers.Add((Renderer)components[i]);
            foreach (Transform child in source) renderers.AddRange(GetAllRenderers(child));
            return renderers.ToArray();
        }

        public Renderer[] UpdateRenderers()
        {
            _renderers = null;
            return renderers;
        }
    }

    public struct UniqueIntList : IEquatable<UniqueIntList>
    {
        int[] ints;
        int length;

        public UniqueIntList(int[] ints, int length)
        {
            this.ints = ints;
            this.length = length;
        }

        public override bool Equals(object other)
        {
            if (!(other is UniqueIntList)) return false;
            return Equals((UniqueIntList)other);
        }

        public bool Equals(UniqueIntList other)
        {
            if (length != other.length) return false;
            for (int i = 0; i < length; i++)
                if (ints[i] != other.ints[i])
                    return false;
            return true;
        }

        public override int GetHashCode()
        {
            int result = 17;
            for (int i = 0; i < length; i++)
                unchecked
                {
                    result = result * 23 + ints[i];
                }
            return result;
        }

        public static bool operator ==(UniqueIntList o1, UniqueIntList o2)
        {
            return o1.Equals(o2);
        }

        public static bool operator !=(UniqueIntList o1, UniqueIntList o2)
        {
            return !o1.Equals(o2);
        }

        public void Add(int i)
        {
            if (ints.Length <= length)
            {
                int[] newInts = new int[length + 1];
                for (int j = 0; j < length; j++)
                    newInts[j] = ints[j];
                ints = newInts;
            }
            ints[length] = i;
            length++;
        }

        public void RemoveLast()
        {
            length--;
        }

        public void ReplaceLast(int i)
        {
            ints[length - 1] = i;
        }

        public void Clear()
        {
            length = 0;
        }

        public int[] ToIntArray()
        {
            int[] newInts = new int[length];
            for (int i = 0; i < length; i++)
                newInts[i] = ints[i];
            return newInts;
        }
    }
}
