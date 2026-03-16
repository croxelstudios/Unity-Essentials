using System;
using UnityEditor;
using UnityEngine;

//TO DO: To update this while inactive, it could maybe subscribe to EditorApplication.Update
//on the constructor?
[ExecuteAlways]
public class StateMachine_ByChildren : StateMachine
{
#if UNITY_EDITOR
    protected override void OnEnable()
    {
        UpdateStates();
        Undo.postprocessModifications += OnPostprocess;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        base.OnEnable();
    }

    void OnDisable()
    {
        Undo.postprocessModifications -= OnPostprocess;
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

    UndoPropertyModification[] OnPostprocess(UndoPropertyModification[] modifications)
    {
        foreach (UndoPropertyModification m in modifications)
        {
            PropertyModification pm = m.currentValue;
            if (pm.target is GameObject go)
            {
                if ((go.transform.parent == transform) &&
                    (pm.propertyPath == "m_Name"))
                    UpdateStates();
            }
        }

        return modifications;
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
