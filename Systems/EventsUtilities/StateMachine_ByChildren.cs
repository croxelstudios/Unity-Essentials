using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class StateMachine_ByChildren : StateMachine
{
#if UNITY_EDITOR
    public StateMachine_ByChildren()
    {
        OnEditorChange.PropertyModification_In(PropertyModification);
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    void OnDestroy()
    {
        OnEditorChange.PropertyModification_Out(PropertyModification);
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
    }

    void Reset()
    {
        UpdateStates();
    }

    protected override void OnEnable()
    {
        UpdateStates();
        base.OnEnable();
    }

    void UpdateStates()
    {
        if (states == null) states = new State[0];
        states = states.Resize(transform.childCount);
        for (int i = 0; i < states.Length; i++)
            states[i].name = transform.GetChild(i).name;
        SyncNames();
        EditorUtility.SetDirty(this);
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }

    void PropertyModification(PropertyModification pm)
    {
        //WARNING: OnDestroy may not correctly unsubscribe the events sometimes
        if ((this != null) && (pm.target is GameObject go))
        {
            if ((go.transform.parent == transform) &&
                (pm.propertyPath == "m_Name"))
                UpdateStates();
        }
    }

    void OnHierarchyChanged()
    {
        //WARNING: OnDestroy may not correctly unsubscribe the events sometimes
        if ((this != null) && ((states == null) || (states.Length != transform.childCount)))
            UpdateStates();
    }
#endif

    protected override void StateSwitchActions(int oldState, int newState)
    {
        transform.GetChild(oldState).gameObject.SetActive(false);
        transform.GetChild(newState).gameObject.SetActive(true);
    }
}

/*
#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(StateMachine_ByChildren))]
public class StateMachine_ByChildren_Inspector : StateMachine_Inspector
{
}
#endif
*/
