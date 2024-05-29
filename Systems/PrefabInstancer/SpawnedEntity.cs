using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SpawnedEntity : MonoBehaviour
{
    public delegate void EntityDelegate(SpawnedEntity entity);
    public event EntityDelegate EntityDestroyed;
    [HideInInspector]
    public PrefabInstancer instancer = null;
    [HideInInspector]
    public PrefabInstancer_Launcher instancerLauncher = null;

    [SerializeField]
    [HideInInspector]
    EventNamesData namesData;
    [SerializeField]
    [HideInInspector]
    DXEvent[] events;
    [SerializeField]
    [HideInInspector]
    EventNamesData instancerNamesData;
    [SerializeField]
    [HideInInspector]
    EventNamesData instancerLauncherNamesData;

    [SerializeField]
    [HideInInspector]
    string[] eventNames;

    void OnDestroy()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            EntityDestroyed?.Invoke(this);
    }

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

    //#if UNITY_EDITOR
    //[HideInInspector]
    //public StringPopupData[] stringPairArray = null;
    //#endif
    [StringPopup("eventNames"/*, "stringPairArray"*/)]
    public void LaunchEvent(int index)
    {
        if ((this.IsActiveAndEnabled()) && (index < events.Length))
            events[index]?.Invoke();
    }

    [StringPopup("instancerNamesData"/*, "stringPairArray"*/)]
    public void LaunchEventInInstancer(int index)
    {
        if (this.IsActiveAndEnabled()) instancer?.FromEntityLaunch(index);
    }

    [StringPopup("instancerLauncherNamesData"/*, "stringPairArray"*/)]
    public void LaunchEventInInstancerLauncher(int index)
    {
        if (this.IsActiveAndEnabled()) instancerLauncher?.FromEntityLaunch(index);
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SpawnedEntity))]
public class SpawnedEntity_Inspector : GenericNamedEvents_Inspector
{
    SerializedProperty namesData;
    SerializedProperty instancerNamesData;
    SerializedProperty instancerLauncherNamesData;
    static bool foldout;

    protected override void OnEnable()
    {
        base.OnEnable();
        namesData = serializedObject.FindProperty("namesData");
        instancerNamesData = serializedObject.FindProperty("instancerNamesData");
        instancerLauncherNamesData = serializedObject.FindProperty("instancerLauncherNamesData");
    }

    protected override void NameArrayChanged(bool priorizeLocal = true)
    {
        base.NameArrayChanged();
        ((SpawnedEntity)target).SyncNames(priorizeLocal);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        foldout = EditorGUILayout.Foldout(foldout, "Remote PrefabInstancer names");
        if (foldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(instancerNamesData);
            EditorGUILayout.PropertyField(instancerLauncherNamesData);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(namesData);
        DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
