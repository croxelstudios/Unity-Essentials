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
#pragma warning disable 414
    [SerializeField]
    EventNamesData entityNamesData = null;
#pragma warning restore 414

    [SerializeField]
    [HideInInspector]
    EventNamesData namesData = null;
    [SerializeField]
    [HideInInspector]
    DXEvent[] events;
    [SerializeField]
    [HideInInspector]
    string[] eventNames;

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
#endif

    public void SyncNames(bool priorizeLocal = false)
    {
        if (namesData != null) namesData.SyncNames(ref eventNames, priorizeLocal);
        //StringPopupData.SyncArray(ref stringPairArray, this);
    }
    //
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
        if (!trackEntitiesLocally) Debug.LogError(gameObject.name + " PrefabInstancer_Launcher can't interact with spawned entities if they are not being locally tracked");
        else if (this.IsActiveAndEnabled())
        {
            for (int i = entities.Count - 1; i >= 0; i--)
                entities[i].gameObject.SetActive(state);
        }
    }

    public void DestroyAllLocalEntities()
    {
        if (!trackEntitiesLocally) Debug.LogError(gameObject.name + " PrefabInstancer_Launcher can't interact with spawned entities if they are not being locally tracked");
        else if (this.IsActiveAndEnabled())
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
    public void LaunchEventInAllTargetEntities(int index)
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref instancer);
            foreach (PrefabInstancer instancer in instancer)
                if (instancer != null) instancer.LaunchEventInAllEntities(index);
        }
    }

    [StringPopup("entityNamesData"/*, "stringPairArray"*/)]
    public void LaunchEventInAllLocalEntities(int index)
    {
        if (!trackEntitiesLocally) Debug.LogError(gameObject.name + " PrefabInstancer can't interact with spawned entities if they are not being tracked");
        else if (this.IsActiveAndEnabled())
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

    void EntityRemoved(SpawnedEntity entity)
    {
        entities.Remove(entity);
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(PrefabInstancer_Launcher))]
public class PrefabInstancer_Launcher_inspector : GenericNamedEvents_Inspector
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
        ((PrefabInstancer_Launcher)target).SyncNames(priorizeLocal);
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
