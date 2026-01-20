using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-1000)]
public class RendererDuplicator : BRendererDuplicator
{
    const string foldoutName = "Along-duplicates modifiers";

    //TO DO: TransformOffsets animated variant for hotline miami effects
    [SerializeField]
    protected GameObject objectToDuplicate = null;
    [SerializeField]
    protected int amountOfDuplicates = 1;
    [SerializeField]
    Transform duplicatesParent = null;
    [ShowIf("@duplicatesParent != null")]
    [Indent]
    [SerializeField]
    bool duplicatesFollowParent = false;
    [SerializeField]
    Component[] extraDuplicableComponents = null;
    [SerializeField]
    bool updateRenderers = false;
    [Header("Replacements")]
    [SerializeField]
    Material[] materialsOverride = null;
    [SerializeField]
    bool replaceAllMaterials = false;
    [SerializeField]
    string replaceLayer = "";
    [SerializeField]
    [SortingLayerSelector]
    string replaceSortingLayer = "";
    [FoldoutGroup(foldoutName)]
    [Header("Renderer")]
    [SerializeField]
    protected int addQueue = 0; //TO DO: Popup with options to select which materials to apply render queue addition to
    [FoldoutGroup(foldoutName)]
    [SerializeField]
    protected int addSortingOrder = 0;
    [Header("Transform")]
    [FoldoutGroup(foldoutName)]
    [SerializeField]
    protected TransformData tranformOffsetMultipliers = new TransformData();
    [FoldoutGroup(foldoutName)]
    [SerializeField]
    protected bool offsetLocally = true;
    [FoldoutGroup(foldoutName)]
    [Header("Color")]
    [SerializeField]
    int colorShiftMaterialIndex = -1;
    [FoldoutGroup(foldoutName)]
    [SerializeField]
    string colorPropertyName = "_BaseColor";
    [FoldoutGroup(foldoutName)]
    [SerializeField]
    RenderersSetColor.BlendMode blendMode = RenderersSetColor.BlendMode.Multiply;
    [FoldoutGroup(foldoutName)]
    [SerializeField]
    protected bool useAlongGradient = true;
    [FoldoutGroup(foldoutName)]
    [SerializeField]
    [ShowIf("useAlongGradient")]
    [Indent]
    Gradient alongGradient = new Gradient()
    {
        alphaKeys = new GradientAlphaKey[]{
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(0f, 1f)
        }
    };

    protected RenderingAgent[] duplicates;
    protected RenderingAgent source;
    protected Dictionary<RenderingAgent, RenderersSetColor> colorSetters;

    bool dynamicParent;

    enum SpawnMode { World, Sibling, Child }

    #region Unity Actions
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void EnableActions()
    {
        base.EnableActions();
        if (materialsOverride != null)
        {
            if (replaceAllMaterials)
            {
                AddRendExclusion("sharedMaterials");
                AddRendExclusion("sharedMaterial");
                AddRendExclusion("materials");
                AddRendExclusion("material");
            }
            else for (int i = 0; i < materialsOverride.Length; i++)
                    if (materialsOverride[i] != null)
                    {
                        AddRendExclusion("sharedMaterials[" + i + "]");
                        AddRendExclusion("materials[" + i + "]");
                        if (i == 0)
                        {
                            AddRendExclusion("sharedMaterial");
                            AddRendExclusion("material");
                        }
                    }
        }
        if (addSortingOrder > 0) AddRendExclusion("sortingOrder");
        source = new RenderingAgent(objectToDuplicate, extraDuplicableComponents);
        CreateDuplicates();
    }

    protected override void DisableActions()
    {
        base.DisableActions();
        if (dynamicParent && (duplicatesParent != null))
        {
            Destroy(duplicatesParent.gameObject);
            dynamicParent = false;
        }
        else
            for (int i = 0; i < duplicates.Length; i++)
                if (duplicates[i].gameObject != null)
                    Destroy(duplicates[i].parent.gameObject);

        duplicates = null;
        colorSetters.Clear();
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        //tranformOffsetMultipliers.hideParent = true;
        if (objectToDuplicate == null)
            objectToDuplicate = gameObject;
    }
#endif

    public override void UpdateEvent()
    {
        //TO DO: Can I avoid transform updates if it's not visible?
        //TO DO: UpdateRenderers might be unnecessary, renderers seem to be recalculated anyway, I don't know why
        if (duplicatesParent.gameObject.activeInHierarchy)
        {
            if (updateRenderers) source.UpdateRenderers();
            UpdateDuplicates(source, ref duplicates);
            AddRendererOrders(duplicates, addSortingOrder, addQueue);
            UpdateDuplicateOffsets();
        }
    }
    #endregion

    #region Duplicates creation
    protected override void RecreateDuplicate(RenderingAgent source, ref RenderingAgent duplicate)
    {
        Transform parent = duplicate.parent.parent;
        colorSetters.Remove(duplicate);

        Destroy(duplicate.parent.gameObject);

        duplicate = CreateDuplicate(source, parent);
        ReplaceRendererData(duplicate, materialsOverride, replaceAllMaterials, replaceLayer);

        int index = GetDuplicateIndex(duplicates, duplicate);
        AddRendererOrders(source, index, duplicate, addSortingOrder, addQueue);
        //Color
        RenderersSetColor bsc = AddBlockSetColor(duplicate);
        colorSetters.Add(duplicate, bsc);
        bsc.SetColor(GetColorOfDuplicate(index));
        //
        UpdateDuplicateOffset(index, duplicate);
    }

    void CreateDuplicates()
    {
        if (duplicatesParent == null)
        {
            duplicatesParent =
                new GameObject("Renderer duplicates of " + objectToDuplicate.name).transform;
            dynamicParent = true;
        }

        duplicates = CreateDuplicates(source, amountOfDuplicates, duplicatesParent);
        if (IsSortingLayerValid(replaceSortingLayer))
            ReplaceSortingLayers(duplicates, replaceSortingLayer);
        ReplaceRenderersData(duplicates, materialsOverride, replaceAllMaterials, replaceLayer);
        AddRendererOrders(duplicates, addSortingOrder, addQueue);

        colorSetters = AddBlockSetColors(duplicates);
        UpdateDuplicateOffsets();
    }

    RenderersSetColor AddBlockSetColor(RenderingAgent duplicate)
    {
        RenderersSetColor bsc = duplicate.gameObject.AddComponent<RenderersSetColor>();
        bsc.Set(true, colorShiftMaterialIndex, colorPropertyName, false);
        bsc.blendMode = blendMode;
        return bsc;
    }

    Dictionary<RenderingAgent, RenderersSetColor> AddBlockSetColors(RenderingAgent[] duplicates)
    {
        Dictionary<RenderingAgent, RenderersSetColor> dic = new Dictionary<RenderingAgent, RenderersSetColor>();
        for (int i = 0; i < duplicates.Length; i++)
        {
            RenderersSetColor bsc = AddBlockSetColor(duplicates[i]);
            if (useAlongGradient) bsc.SetColor(GetColorOfDuplicate(i));
            dic.Add(duplicates[i], bsc);
        }
        return dic;
    }

    protected Color GetColorOfDuplicate(int id)
    {
        return alongGradient.Evaluate(Mathf.InverseLerp(0f, duplicates.Length - 1, id));
    }
    #endregion

    #region Helpers
    protected void AddRendererOrders(RenderingAgent source, RenderingAgent duplicate,
        int sortOrderAdd, int queueAdd = 0)
    {
        Renderer[] sources = source.renderers;
        Renderer[] duplicates = duplicate.renderers;
        for (int i = 0; i < sources.Length; i++)
        {
            if (queueAdd != 0)
            {
                Material[] shM = sources[i].sharedMaterials;
                for (int j = 0; j < shM.Length; j++)
                    duplicates[i].materials[j].renderQueue = shM[j].renderQueue + queueAdd;
            }
            duplicates[i].sortingOrder = sources[i].sortingOrder + sortOrderAdd;
        }
    }

    protected void AddRendererOrders(RenderingAgent source, int index, RenderingAgent duplicate,
        int sortOrderMultiplier, int queueMultiplier = 0)
    {
        AddRendererOrders(source, duplicate,
            (index + 1) * sortOrderMultiplier, (index + 1) * queueMultiplier);
    }

    protected void AddRendererOrders(RenderingAgent[] duplicates,
        int sortOrderMultiplier, int queueMultiplier = 0)
    {
        for (int i = 0; i < duplicates.Length; i++)
            AddRendererOrders(source, i, duplicates[i], sortOrderMultiplier, queueMultiplier);
    }

    protected void UpdateDuplicateOffset(int index, RenderingAgent duplicate,
        bool checkFollowParent = true, bool reset = true)
    {
        UpdateDuplicateOffset(index, duplicate,
            ProcessOffset(tranformOffsetMultipliers), offsetLocally,
            checkFollowParent, reset);
    }

    protected void UpdateDuplicateOffsets(bool checkFollowParent = true, bool reset = true)
    {
        UpdateDuplicateOffsets(ProcessOffset(tranformOffsetMultipliers), offsetLocally,
            checkFollowParent, reset);
    }

    protected void UpdateDuplicateOffset(int index, RenderingAgent duplicate,
        TransformData trOffset, bool offsetLocally, bool checkFollowParent = true, bool reset = true)
    {
        bool followParent = checkFollowParent && duplicatesFollowParent;
        CheckFollowParent(duplicates[0], ref followParent,
            out TransformData parentData, out Transform parent);

        UpdateDuplicateOffset(objectToDuplicate.transform,
            index, duplicate, trOffset, offsetLocally, reset);

        if (followParent)
            parentData.SetInTransform(parent, true);
    }

    protected void UpdateDuplicateOffsets(
        TransformData trOffset, bool offsetLocally, bool checkFollowParent = true, bool reset = true)
    {
        if (!duplicates.IsNullOrEmpty())
        {
            bool followParent = checkFollowParent && duplicatesFollowParent;
            CheckFollowParent(duplicates[0], ref followParent,
                out TransformData parentData, out Transform parent);

            UpdateDuplicateOffsets(objectToDuplicate.transform,
                duplicates, trOffset, offsetLocally, reset);

            if (followParent)
                parentData.SetInTransform(parent, true);
        }
    }

    protected bool CheckFollowParent(RenderingAgent duplicate,
        out TransformData parentData, out Transform parent)
    {
        bool followParent = duplicatesFollowParent;
        CheckFollowParent(duplicate, ref followParent, out parentData, out parent);
        return followParent;
    }

    protected void CheckFollowParent(RenderingAgent duplicate,
        ref bool followParent, out TransformData parentData, out Transform parent)
    {
        CheckFollowParent(objectToDuplicate.transform,
            duplicate, ref followParent, out parentData, out parent);
    }

    void CheckFollowParent(Transform source, RenderingAgent duplicate,
        ref bool followParent, out TransformData parentData, out Transform parent)
    {
        parentData = new TransformData();
        parent = duplicate.parent.parent;
        if (followParent && (parent != null))
        {
            parentData = new TransformData(parent, true);
            parent.position = source.position;
            parent.eulerAngles = Vector3.zero;
            parent.localScale = Vector3.one;
        }
        else followParent = false;
    }

    protected TransformData ProcessOffset(TransformData tranformOffsetMultipliers, bool lerp = true)
    {
        TransformData toLerp = new TransformData();
        if (tranformOffsetMultipliers.parent == null)
            toLerp = tranformOffsetMultipliers;
        else
        {
            toLerp = new TransformData(tranformOffsetMultipliers.parent);
            Transform origin =
                (duplicatesFollowParent && (duplicatesParent != null)) ?
                duplicatesParent : objectToDuplicate.transform;
            toLerp.Subtract(origin);
            toLerp.Add(tranformOffsetMultipliers);
        }
        return lerp ? toLerp.LerpFromZero(1f / duplicates.Length) : toLerp;
    }
    #endregion

    #region Public functions
    public void SetDuplicatesActiveState(bool state)
    {
        if (this.IsActiveAndEnabled())
        {
            for (int i = 0; i < duplicates.Length; i++)
                SetDuplicateActiveState(i, state);
        }
    }

    public void SetDuplicateActiveState(int id, bool state)
    {
        if (this.IsActiveAndEnabled())
        {
            duplicates[id].gameObject.SetActive(state);
        }
    }

    public void ForceComponentReset()
    {
        if (this.IsActiveAndEnabled())
        {
            DisableActions();
            source = new RenderingAgent(objectToDuplicate, extraDuplicableComponents);
            EnableActions();
        }
    }
    #endregion
}
