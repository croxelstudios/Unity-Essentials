using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class PrefabInstancer_Launcher : BRemoteLauncher
{
    PrefabInstancer[] instancer;

    [SerializeField]
    bool trackEntitiesLocally = false;
    [SerializeField]
    GameObject[] prefabOverrides = null;

    List<SpawnedEntity> entities;

    void Awake()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            FillArrayAwake(ref instancer);
            if (trackEntitiesLocally) entities = new List<SpawnedEntity>();
        }
    }

    public void InstantiatePrefab(GameObject prefab)
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref instancer);
            foreach (PrefabInstancer instancer in instancer)
                if (instancer != null)
                {
                    if (trackEntitiesLocally)
                    {
                        SpawnedEntity entity = instancer.Instantiate(prefab, true);
                        entities.Add(entity);
                        entity.EntityDestroyed += EntityRemoved;
                        entity.instancerLauncher = this;
                    }
                    else instancer.Instantiate(prefab, false);
                }
        }
    }

    public void InstantiatePrefab(int n)
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref instancer);
            foreach (PrefabInstancer instancer in instancer)
                if (instancer != null)
                {
                    if (trackEntitiesLocally)
                    {
                        SpawnedEntity entity = instancer.Instantiate(n, true, prefabOverrides);
                        entities.Add(entity);
                        entity.EntityDestroyed += EntityRemoved;
                        entity.instancerLauncher = this;
                    }
                    else instancer.Instantiate(n, false, prefabOverrides);
                }
        }
    }

    public void InstantiateRandomPrefab()
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref instancer);
            foreach (PrefabInstancer instancer in instancer)
                if (instancer != null)
                {
                    if (trackEntitiesLocally)
                    {
                        SpawnedEntity entity = instancer.InstantiateRandom(true, prefabOverrides);
                        entities.Add(entity);
                        entity.EntityDestroyed += EntityRemoved;
                        entity.instancerLauncher = this;
                    }
                    else instancer.InstantiateRandom(false, prefabOverrides);
                }
        }
    }

    public void InstantiatePrefabs()
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref instancer);
            foreach (PrefabInstancer instancer in instancer)
                if (instancer != null)
                {
                    if (trackEntitiesLocally)
                    {
                        SpawnedEntity[] newEntities = instancer.InstantiateAll(true, prefabOverrides);
                        entities.AddRange(newEntities);
                        for (int i = 0; i < newEntities.Length; i++)
                        {
                            newEntities[i].EntityDestroyed += EntityRemoved;
                            newEntities[i].instancerLauncher = this;
                        }
                    }
                    else instancer.InstantiateAll(false, prefabOverrides);
                }
        }
    }

    public void SetActiveAllTargetEntities(bool state)
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref instancer);
            foreach (PrefabInstancer instancer in instancer)
                if (instancer != null) instancer.SetActiveAllEntities(state);
        }
    }

    public void DestroyAllTargetEntities()
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref instancer);
            foreach (PrefabInstancer instancer in instancer)
                if (instancer != null) instancer.DestroyAllEntities();
        }
    }

    public void SetActiveAllLocalEntities(bool state)
    {
        if (!trackEntitiesLocally)
            Debug.LogError(gameObject.name +
                " PrefabInstancer_Launcher can't interact with spawned entities" +
                "if they are not being locally tracked");
        else if (this.IsActiveAndEnabled())
        {
            for (int i = entities.Count - 1; i >= 0; i--)
                entities[i].gameObject.SetActive(state);
        }
    }

    public void DestroyAllLocalEntities()
    {
        if (!trackEntitiesLocally)
            Debug.LogError(gameObject.name +
                " PrefabInstancer_Launcher can't interact with spawned entities" +
                "if they are not being locally tracked");
        else if (this.IsActiveAndEnabled())
        {
            for (int i = entities.Count - 1; i >= 0; i--)
                Destroy(entities[i].gameObject);
        }
    }

    void EntityRemoved(SpawnedEntity entity)
    {
        entities.Remove(entity);
    }

    #region Target-filtered signals
    public void CallSignalOnTargetEntities(EventSignal signal)
    {
        FillArrayUpdate(ref instancer);
        foreach (PrefabInstancer instancer in instancer)
            if (instancer != null) instancer.CallSignalOnEntities(signal);
    }
    #endregion

    #region Launcher-filtered signals
    public void CallSignalOnLocalEntities(EventSignal signal)
    {
        if (!trackEntitiesLocally)
            Debug.LogError(gameObject.name +
                " PrefabInstancer_Launcher can't interact with spawned entities if they are not being locally tracked");
        else if (this.IsActiveAndEnabled())
            signal.CallSignal(entities.GetTransforms());
    }
    #endregion
}
