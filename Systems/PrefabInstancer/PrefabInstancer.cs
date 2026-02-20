using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;

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
    [HideIf("spawnMode", SpawnMode.World)]
    Transform customReference = null;
    [SerializeField]
    bool trackEntities = false;
    [Indent]
    [ShowIf("trackEntities")]
    [SerializeField]
    int maxEntityCount = 5;
    [Indent]
    [ShowIf("trackEntities")]
    [SerializeField]
    TimeMode momentumTimeMode = TimeMode.FixedUpdate;

    WeightedPrefab[] wPrefabs;
    public List<SpawnedEntity> entities;

    bool wPrefabsFilled = false;
    Vector3 prevPos;
    Vector3 momentum;

    protected virtual void OnEnable()
    {
        prevPos = transform.position;
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
    }

    void Update()
    {
        if (momentumTimeMode.IsSmooth())
            OnUpdate(momentumTimeMode.DeltaTime());
    }

    void FixedUpdate()
    {
        if (momentumTimeMode.IsFixed())
            OnUpdate(momentumTimeMode.DeltaTime());
    }

    void LateUpdate()
    {
        if (momentumTimeMode.IsSmooth())
            OnLateUpdate(momentumTimeMode.DeltaTime());
        else StartCoroutine(LateFixedUpdate());
    }

    IEnumerator LateFixedUpdate()
    {
        yield return new WaitForFixedUpdate();
        OnLateUpdate(momentumTimeMode.DeltaTime());
    }

    protected virtual void OnUpdate(float deltaTime)
    {
    }

    protected virtual void OnLateUpdate(float deltaTime)
    {
        momentum = (transform.position - prevPos) / deltaTime;
        prevPos = transform.position;
    }

    void OnDestroy()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            if (trackEntities && (entities != null)) foreach (SpawnedEntity entity in entities)
                    entity.EntityDestroyed -= EntityRemoved;
    }

    void FillWeightedPrefabs()
    {
        wPrefabs = new WeightedPrefab[weightedPrefabs.Length + prefabs.Length];
        for (int i = 0; i < wPrefabs.Length; i++)
        {
            if (i < prefabs.Length) wPrefabs[i] = new WeightedPrefab(prefabs[i], 1f);
            else wPrefabs[i] = weightedPrefabs[i - prefabs.Length];
        }
    }

    #region Public functions
    public void InstantiatePrefab(GameObject prefab)
    {
        if (this.IsActiveAndEnabled())
            InstantiatePrefabDisabled(prefab);
    }

    public void InstantiatePrefabDisabled(GameObject prefab)
    {
        int amount = Random.Range(amountRange.x, amountRange.y);
        for (int i = 0; i < amount; i++)
            Instantiate(prefab);
    }

    public void InstantiatePrefab(int n)
    {
        if (this.IsActiveAndEnabled())
            InstantiatePrefabDisabled(n);
    }

    public void InstantiatePrefabDisabled(int n)
    {
        int amount = Random.Range(amountRange.x, amountRange.y);
        for (int i = 0; i < amount; i++)
            Instantiate(n);
    }

    public void InstantiateRandomPrefab()
    {
        if (this.IsActiveAndEnabled())
            InstantiateRandomPrefabDisabled();
    }

    public void InstantiateRandomPrefabDisabled()
    {
        int amount = Random.Range(amountRange.x, amountRange.y);
        for (int i = 0; i < amount; i++)
            InstantiateRandom();
    }

    public void InstantiatePrefabs()
    {
        if (this.IsActiveAndEnabled())
            InstantiatePrefabsDisabled();
    }

    public void InstantiatePrefabsDisabled()
    {
        InstantiateAll();
    }

    public void SetActiveAllEntities(bool state)
    {
        if (!trackEntities)
            Debug.LogError(gameObject.name +
                " PrefabInstancer can't interact with spawned entities if they are not being tracked");
        else if (this.IsActiveAndEnabled() && (entities != null))
        {
            for (int i = entities.Count - 1; i >= 0; i--)
                entities[i].gameObject.SetActive(state);
        }
    }

    public void DestroyAllEntities()
    {
        if (!trackEntities)
            Debug.LogError(gameObject.name +
                " PrefabInstancer can't interact with spawned entities if they are not being tracked");
        else if (this.IsActiveAndEnabled() && (entities != null))
        {
            for (int i = entities.Count - 1; i >= 0; i--)
                Destroy(entities[i].gameObject);
        }
    }
    #endregion

    #region Entity-filtered signals
    public void CallSignalOnEntities(EventSignal signal)
    {
        if (!trackEntities)
            Debug.LogError(gameObject.name +
                " PrefabInstancer can't interact with spawned entities if they are not being tracked");
        else if (this.IsActiveAndEnabled() && (entities != null))
        {
            signal.CallSignal(entities.GetTransforms());
        }
    }
    #endregion

    #region Tracked Entities Functions
    public virtual SpawnedEntity Instantiate(GameObject prefab, bool ignoreMax = false, bool forceTracking = false)
    {
        if ((prefab != null) && CanSpawn((!ignoreMax) && (forceTracking || trackEntities)))
        {
            GameObject instance = InstantiateWithMode(prefab, spawnMode);
            if (forceTracking || trackEntities)
            {
                SpawnedEntity entity = instance.GetComponent<SpawnedEntity>();
                if (entity == null) entity = instance.AddComponent<SpawnedEntity>();
                if (trackEntities)
                {
                    entities = entities.CreateAdd(entity);
                    entity.EntityDestroyed += EntityRemoved;
                    entity.instancer = this;
                }
                entity.LaunchMomentum(momentum);
                return entity;
            }
        }

        return null;
    }

    bool CanSpawn(bool trackEntities)
    {
        if (trackEntities)
        {
            entities = entities.CreateIfNull();
            return entities.Count < maxEntityCount;
        }
        else return true;
    }

    public virtual SpawnedEntity Instantiate(int n, bool ignoreMax = false, bool forceTracking = false, GameObject[] prefabOverrides = null)
    {
        GameObject prefab = GetPrefabFromArray(n, prefabOverrides);
        return Instantiate(prefab, ignoreMax, forceTracking);
    }

    public SpawnedEntity InstantiateRandom(bool ignoreMax = false, bool forceTracking = false, GameObject[] prefabOverrides = null)
    {
        if (!TryFillUpArray()) return null;

        return Instantiate(ChooseWeightedPrefab(wPrefabs), ignoreMax, forceTracking, prefabOverrides);
    }

    public SpawnedEntity[] InstantiateAll(bool forceTracking = false, GameObject[] prefabOverrides = null)
    {
        if (!TryFillUpArray()) return null;

        SpawnedEntity[] array = new SpawnedEntity[wPrefabs.Length];
        for (int i = 0; i < wPrefabs.Length; i++)
            array[i] = Instantiate(i, true, forceTracking, prefabOverrides);
        return array;
    }
    #endregion

    GameObject GetPrefabFromArray(int n, GameObject[] prefabOverrides = null)
    {
        if (!TryFillUpArray()) return null;

        GameObject prefab = ((!prefabOverrides.IsNullOrEmpty()) && (n < prefabOverrides.Length)) ?
            prefabOverrides[n] : wPrefabs[n].prefab;
        return prefab;
    }

    bool TryFillUpArray()
    {
        if (!wPrefabsFilled) FillWeightedPrefabs();
        if (wPrefabs.IsNullOrEmpty()) return false;
        else return true;
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
        entities.SmartRemove(entity);
    }
    #endregion
}
