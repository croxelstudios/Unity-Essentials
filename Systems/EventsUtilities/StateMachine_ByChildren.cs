using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//TO DO: To update this while inactive, it could maybe subscribe to EditorApplication.Update
//on the constructor?
[ExecuteAlways]
public class StateMachine_ByChildren : StateMachine
{
#if UNITY_EDITOR
    private void Reset()
    {
        UpdateStates();
    }

    protected override void OnEnable()
    {
        UpdateStates();
        OnEditorChange.PropertyModification_In(PropertyModification);
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        base.OnEnable();
    }

    void OnDisable()
    {
        OnEditorChange.PropertyModification_Out(PropertyModification);
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
    }

    void UpdateStates()
    {
        if (states == null) states = new State[0];
        states = states.Resize(transform.childCount);
        for (int i = 0; i < states.Length; i++)
        {
            states[i].SetStateMachine(this);
            states[i].name = transform.GetChild(i).name;
        }
        SyncNames();
        EditorUtility.SetDirty(this);
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }

    void PropertyModification(PropertyModification pm)
    {
        if (pm.target is GameObject go)
        {
            if ((go.transform.parent == transform) &&
                (pm.propertyPath == "m_Name"))
                UpdateStates();
        }
    }

    void OnHierarchyChanged()
    {
        if ((states == null) || (states.Length != transform.childCount))
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
