using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-1000)]
public class RendererDuplicator : BRendererDuplicator
{
    //TO DO: TransformOffsets animated variant for hotline miami effects

    [SerializeField]
    Component[] extraDuplicableComponents = null;
    [SerializeField]
    protected GameObject objectToDuplicate = null;
    [SerializeField]
    bool updateRenderers = false;
    [SerializeField]
    protected int amountOfDuplicates = 1;
    // TO DO: This is broken
    //[SerializeField]
    //Material[] materialsOverride = null;
    //[SerializeField]
    //bool replaceAllMaterials = false;
    [SerializeField]
    Transform duplicatesParent = null;
    [SerializeField]
    string replaceLayer = "";
    [Header("Renderer multipliers")]
    [SerializeField]
    int queueMultiplier = 0; //TO DO: Popup with options to select which materials to apply render queue addition to
    [SerializeField]
    [SortingLayerSelector]
    string replaceSortingLayer = "";
    [SerializeField]
    int sortingOrderMultiplier = 0;
    [SerializeField]
    protected TransformData tranformOffsetMultipliers = new TransformData();
    [SerializeField]
    protected bool offsetLocally = true;
    [Header("Color settings")]
    [SerializeField]
    int colorShiftMaterialIndex = -1;
    [SerializeField]
    string colorPropertyName = "_Color";
    [SerializeField]
    RenderersSetColor.BlendMode blendMode = RenderersSetColor.BlendMode.Multiply;
    [SerializeField]
    protected bool useAlongGradient = true;
    [SerializeField]
    [ShowIf("useAlongGradient")]
    Gradient alongGradient = new Gradient();

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

    void OnValidate()
    {
        if (objectToDuplicate == null)
            objectToDuplicate = gameObject;
    }

    public override void UpdateEvent()
    {
        //TO DO: Can I avoid transform updates if it's not visible?
        //TO DO: UpdateRenderers might be unnecessary, renderers seem to be recalculated anyway, I don't know why
        if (duplicatesParent.gameObject.activeInHierarchy)
        {
            if (updateRenderers) source.UpdateRenderers();
            UpdateDuplicates(source, ref duplicates);
            UpdateDuplicateOffsets(objectToDuplicate.transform, duplicates, offsetLocally, tranformOffsetMultipliers);
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
        // TO DO: Broken
        //ReplaceRendererData(duplicate, materialsOverride, queueMultiplier, sortingOrderMultiplier, replaceAllMaterials, replaceLayer);
        //Color
        RenderersSetColor bsc = AddMaterialSetColor(duplicate);
        colorSetters.Add(duplicate, bsc);
        bsc.SetColor(GetColorOfDuplicate(GetDuplicateIndex(duplicates, duplicate)));
        //
        UpdateDuplicateOffset(objectToDuplicate.transform, duplicate, offsetLocally, tranformOffsetMultipliers);
    }

    void CreateDuplicates()
    {
        if (duplicatesParent == null)
        {
            duplicatesParent = new GameObject("Renderer duplicates of " + objectToDuplicate.name).transform;
            dynamicParent = true;
        }

        duplicates = CreateDuplicates(source, amountOfDuplicates, duplicatesParent);
        if (IsSortingLayerValid(replaceSortingLayer))
            ReplaceSortingLayers(duplicates, replaceSortingLayer);
        // TO DO: Broken
        //ReplaceRenderersData(duplicates, materialsOverride, queueMultiplier, sortingOrderMultiplier, replaceAllMaterials, replaceLayer);

        colorSetters = AddBlockSetColors(duplicates);
        UpdateDuplicateOffsets(objectToDuplicate.transform, duplicates, offsetLocally, tranformOffsetMultipliers);
    }

    RenderersSetColor AddMaterialSetColor(RenderingAgent duplicate)
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
            RenderersSetColor bsc = AddMaterialSetColor(duplicates[i]);
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
