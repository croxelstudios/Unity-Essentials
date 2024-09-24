using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class PrefabInstancer : MonoBehaviour
{
    [SerializeField]
    [Tooltip("These are also counted in the weighted calculation and count as weight 1")]
    GameObject[] prefabs = null;
    [SerializeField]
    WeightedPrefab[] weightedPrefabs = null;
    [SerializeField]
    bool inheritRotation = true;
    [SerializeField]
    Vector2Int amountRange = new Vector2Int(1, 1);
    [SerializeField]
    Vector3 rotationVariation = Vector3.zero;
    [SerializeField]
    Vector3 randomSpread = Vector3.zero;
    [SerializeField]
    SpawnMode spawnMode = SpawnMode.World;
    [SerializeField]
    [HideIf("@this.spawnMode == SpawnMode.World")]
    Transform customReference = null;
    [SerializeField]
    bool trackEntities = false;

#if UNITY_EDITOR
#pragma warning disable 414
    [SerializeField]
    EventNamesData entityNamesData = null;
#pragma warning restore 414
    [SerializeField]
    [HideInInspector]
    EventNamesData namesData = null;
    [SerializeField]
    [HideInInspector]
    string[] eventNames;
#endif
    [SerializeField]
    [HideInInspector]
    DXEvent[] events;
    //TO DO: Support for keeping momentum of original object (DXVectorEvent originalMomentum on instance)

    WeightedPrefab[] wPrefabs;
    public List<SpawnedEntity> entities = new List<SpawnedEntity>();

    bool wPrefabsFilled = false;

    void Awake()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            if (!wPrefabsFilled)
                FillWeightedPrefabs();
        }
    }

    void OnDestroy()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            if (trackEntities && (entities != null)) foreach (SpawnedEntity entity in entities)
                    entity.EntityDestroyed -= EntityRemoved;
    }

    //
#if UNITY_EDITOR
    bool hasValidated = false;

    void OnValidate()
    {
        if ((!Application.isPlaying) && (!hasValidated))
        {
            SyncNames();
            hasValidated = true;
        }
    }

    void Update() //TO DO: Should execute even when disabled
    {
        if (!Application.isPlaying) SyncNames();
    }

    public void SyncNames(bool priorizeLocal = false)
    {
        if (namesData != null) namesData.SyncNames(ref eventNames, priorizeLocal);
        //StringPopupData.SyncArray(ref stringPairArray, this);
    }
#endif
    //

    #region Public functions
    public void InstantiatePrefab(GameObject prefab)
    {
        if (this.IsActiveAndEnabled())
        {
            if (!wPrefabsFilled)
            {
                FillWeightedPrefabs();
                int amount = Random.Range(amountRange.x, amountRange.y);
                for (int i = 0; i < amount; i++)
                    Instantiate(prefab);
            }
        }
    }

    public void InstantiatePrefab(int n)
    {
        if (this.IsActiveAndEnabled())
        {
            if (!wPrefabsFilled)
            {
                FillWeightedPrefabs();
                int amount = Random.Range(amountRange.x, amountRange.y);
                for (int i = 0; i < amount; i++)
                    Instantiate(n);
            }
        }
    }

    public void InstantiateRandomPrefab()
    {
        if (this.IsActiveAndEnabled() && (wPrefabs != null))
        {
            if (!wPrefabsFilled)
            {
                FillWeightedPrefabs();
                int amount = Random.Range(amountRange.x, amountRange.y);
                for (int i = 0; i < amount; i++)
                    InstantiateRandom();
            }
        }
    }

    public void InstantiatePrefabs()
    {
        if (this.IsActiveAndEnabled())
        {
            if (!wPrefabsFilled)
            {
                FillWeightedPrefabs();
                InstantiateAll();
            }
        }
    }

    public void InstantiatePrefabsDisabled()
    {
        if (!wPrefabsFilled)
        {
            FillWeightedPrefabs();
            InstantiateAll();
        }
    }

    public void SetActiveAllEntities(bool state)
    {
        if (!trackEntities) Debug.LogError(gameObject.name + " PrefabInstancer can't interact with spawned entities if they are not being tracked");
        else if (this.IsActiveAndEnabled() && (entities != null))
        {
            for (int i = entities.Count - 1; i >= 0; i--)
                entities[i].gameObject.SetActive(state);
        }
    }

    public void DestroyAllEntities()
    {
        if (!trackEntities) Debug.LogError(gameObject.name + " PrefabInstancer can't interact with spawned entities if they are not being tracked");
        else if (this.IsActiveAndEnabled() && (entities != null))
        {
            for (int i = entities.Count - 1; i >= 0; i--)
                Destroy(entities[i].gameObject);
        }
    }

    //#if UNITY_EDITOR
    //[HideInInspector]
    //public StringPopupData[] stringPairArray = null;
    //#endif
    [StringPopup("entityNamesData"/*, "stringPairArray"*/)]
    public void LaunchEventInAllEntities(int index)
    {
        if (!trackEntities) Debug.LogError(gameObject.name + " PrefabInstancer can't interact with spawned entities if they are not being tracked");
        else if (this.IsActiveAndEnabled() && (entities != null))
        {
            for (int i = entities.Count - 1; i >= 0; i--)
                entities[i].LaunchEvent(index);
        }
    }

    [StringPopup("eventNames"/*, "stringPairArray"*/)]
    public void FromEntityLaunch(int index)
    {
        if (this.IsActiveAndEnabled()) events[index]?.Invoke();
    }
    #endregion

    #region Tracked Entities Functions

    void FillWeightedPrefabs()
    {
        wPrefabs = new WeightedPrefab[weightedPrefabs.Length + prefabs.Length];
        for (int i = 0; i < wPrefabs.Length; i++)
        {
            if (i < prefabs.Length) wPrefabs[i] = new WeightedPrefab(prefabs[i], 1f);
            else wPrefabs[i] = weightedPrefabs[i - prefabs.Length];
        }
    }
    public virtual SpawnedEntity Instantiate(GameObject prefab, bool forceTracking = false)
    {
        if (prefab != null)
        {
            GameObject instance = InstantiateWithMode(prefab, spawnMode);
            if (forceTracking || trackEntities)
            {
                SpawnedEntity entity = instance.GetComponent<SpawnedEntity>();
                if (entity == null) entity = instance.AddComponent<SpawnedEntity>();
                if (trackEntities)
                {
                    if (entities == null) entities = new List<SpawnedEntity>();
                    entities.Add(entity);
                    entity.EntityDestroyed += EntityRemoved;
                    entity.instancer = this;
                }
                return entity;
            }
        }

        return null;
    }

    public virtual SpawnedEntity Instantiate(int n, bool forceTracking = false, GameObject[] prefabOverrides = null)
    {
        GameObject prefab = GetPrefabFromArray(n, prefabOverrides);
        return Instantiate(prefab, forceTracking);
    }

    public SpawnedEntity InstantiateRandom(bool forceTracking = false, GameObject[] prefabOverrides = null)
    {
        return Instantiate(ChooseWeightedPrefab(wPrefabs), forceTracking, prefabOverrides);
    }

    public SpawnedEntity[] InstantiateAll(bool forceTracking = false, GameObject[] prefabOverrides = null)
    {
        SpawnedEntity[] array = new SpawnedEntity[wPrefabs.Length];
        for (int i = 0; i < wPrefabs.Length; i++)
            array[i] = Instantiate(i, forceTracking, prefabOverrides);
        return array;
    }
    #endregion

    GameObject GetPrefabFromArray(int n, GameObject[] prefabOverrides = null)
    {
        GameObject prefab = ((prefabOverrides != null) && (n < prefabOverrides.Length)) ?
            prefabOverrides[n] : wPrefabs[n].prefab;
        return prefab;
    }

    GameObject InstantiateWithMode(GameObject prefab, SpawnMode spawnMode)
    {
        Vector3 random = Vector3.one.GetRandom();
        Vector3 spawnPosition = transform.position + Vector3.Scale(random, randomSpread);
        GameObject instance;
        switch (spawnMode)
        {
            case SpawnMode.Sibling:
                instance = Instantiate(prefab, spawnPosition,
                    Quaternion.Euler(rotationVariation.GetRandom()) *
                    (inheritRotation ? transform.rotation : prefab.transform.rotation),
                    customReference != null ? customReference.parent : transform.parent);
                break;
            case SpawnMode.Child:
                instance = Instantiate(prefab, spawnPosition,
                    Quaternion.Euler(rotationVariation.GetRandom()) *
                    (inheritRotation ? transform.rotation : prefab.transform.rotation),
                    customReference != null ? customReference : transform);
                break;
            default:
                instance = Instantiate(prefab, spawnPosition,
                    Quaternion.Euler(rotationVariation.GetRandom()) *
                    (inheritRotation ? transform.rotation : prefab.transform.rotation),
                    transform);
                instance.transform.SetParent(null);
                break;
        }
        return instance;
    }

    #region Utilities
    int ChooseWeightedPrefab(WeightedPrefab[] array)
    {
        float totalWeight = 0f;
        for (int i = 0; i < array.Length; i++) totalWeight += array[i].weight;

        int chosen = -1;
        float currentWeight = Random.Range(0, totalWeight);
        for (int i = 0; i < array.Length; i++)
        {
            if (currentWeight <= array[i].weight)
            {
                chosen = i;
                break;
            }
            else currentWeight -= array[i].weight;
        }

        return chosen;
    }

    enum SpawnMode { World, Sibling, Child }

    [Serializable]
    struct WeightedPrefab
    {
        public GameObject prefab;
        public float weight;

        public WeightedPrefab(GameObject prefab, float weight)
        {
            this.prefab = prefab;
            this.weight = weight;
        }
    }

    void EntityRemoved(SpawnedEntity entity)
    {
        entities.Remove(entity);
    }
    #endregion
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(PrefabInstancer))]
public class PrefabInstancer_Inspector : GenericNamedEvents_Inspector
{
    SerializedProperty namesData;
    static bool foldout;

    protected override void OnEnable()
    {
        base.OnEnable();
        namesData = serializedObject.FindProperty("namesData");
    }

    protected override void NameArrayChanged(bool priorizeLocal = true)
    {
        base.NameArrayChanged();
        ((PrefabInstancer)target).SyncNames(priorizeLocal);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        foldout = EditorGUILayout.Foldout(foldout, "From entity actions");
        if (foldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(namesData);
            DoLayoutList();
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
