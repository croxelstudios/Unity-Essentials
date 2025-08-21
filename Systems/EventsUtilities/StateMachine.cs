using UnityEngine;
using System;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StateMachine : MonoBehaviour
{
    [PropertyOrder(-1)]
    public StateMachine connectedStateMachine = null;
    [SerializeField]
    bool launchOnEnable = false;
    [SerializeField]
    bool lastLinkedObjectsRemain = false;
    [SerializeField]
    [StringPopup("stateNames")]
    [OnValueChanged("UpdateState")]
    int _currentState = 0;

    int _initialState;

    private void Awake()
    {
        _initialState = _currentState;
    }

    void OnEnable()
    {
        if (launchOnEnable)
            EnterStateAction();
    }

    public int currentState
    {
        get { return _currentState; }
        set
        {
            if (_currentState != value)
            {
                if ((value < states.Length) && (value >= 0))
                {
                    SwitchState(value);
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        RecordGameObjectModificationsFromPrefab();
#endif
                }
            }
        }
    }

    [OnValueChanged("SyncNames", true)]
    [OnValueChanged("ClampCurrentState")]
    public State[] states = null;

    [HideInInspector]
    public string[] stateNames;
    int prevState = 0;

#if UNITY_EDITOR
    StateMachine prevConnectedSM;
    bool wasprevStateUpdated = false;

    [ShowIf("@connectedStateMachine != null")]
    [PropertyOrder(-1)]
    [Button]
    public void UpdateStatesFromConnectedMachine()
    {
        states.Resize(connectedStateMachine.states.Length);
        stateNames.Resize(states.Length);
        for (int i = 0; i < states.Length; i++)
        {
            states[i].name = connectedStateMachine.states[i].name;
            stateNames[i] = states[i].name;
        }
    }

    public virtual void OnValidate()
    {
        if (!wasprevStateUpdated)
        {
            prevState = _currentState;
            wasprevStateUpdated = true;
        }

        if (connectedStateMachine != null)
        {
            if (prevConnectedSM != connectedStateMachine)
            {
                UpdateStatesFromConnectedMachine();
                prevConnectedSM = connectedStateMachine;
            }

            State[] newStates = new State[states.Length];
            if (connectedStateMachine.states == null)
                connectedStateMachine.states = new State[states.Length];
            for (int i = 0; i < states.Length; i++)
            {
                newStates[i].name = states[i].name;
                if (i < connectedStateMachine.states.Length)
                {
                    newStates[i].enter = connectedStateMachine.states[i].enter;
                    newStates[i].exit = connectedStateMachine.states[i].exit;
                }
            }
            connectedStateMachine.states = newStates;
            connectedStateMachine.OnValidate();
        }
        //StringPopupData.SyncArray(ref stringPairArray, this);
    }

    void RecordGameObjectModificationsFromPrefab()
    {
        EditorUtility.SetDirty(gameObject);
        Component[] components = GetComponentsInChildren<Component>();
        foreach (Component component in components)
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
    }
#endif

    //#if UNITY_EDITOR
    //[HideInInspector]
    //public StringPopupData[] stringPairArray = null;
    //#endif
    [StringPopup("stateNames"/*, "stringPairArray"*/)]
    public virtual void SwitchState(int newState)
    {
        newState = Mathf.Clamp(newState, 0, states.Length - 1);
        if (
             //(this.IsActiveAndEnabled() ||
             //#if UNITY_EDITOR
             //            (!Application.isPlaying)
             //#endif
             //            ) &&
             (_currentState != newState))
        {
            ExitStateAction();
            StateSwitchActions(_currentState, newState);
            _currentState = newState;
            if (connectedStateMachine != null)
                connectedStateMachine.SwitchState(newState);
            EnterStateAction();
        }
    }

    void ExitStateAction()
    {
        if (_currentState < states.Length && _currentState >= 0)
        {
            if (states[_currentState].linkedObjects != null)
                if ((_currentState < (states.Length - 1)) || !lastLinkedObjectsRemain)
                    //WARNING: For some reason it turns null in _ByChildren variant
                    foreach (GameObject obj in states[_currentState].linkedObjects)
                        obj.SetActive(false);
            states[_currentState].exit?.Invoke();
        }
    }

    void EnterStateAction()
    {
        if (_currentState < states.Length)
        {
            if (states[_currentState].linkedObjects != null)
                foreach (GameObject obj in states[_currentState].linkedObjects)
                    obj.SetActive(true);
            states[_currentState].enter?.Invoke();
        }
    }

    public void NextState()
    {
        SwitchState((currentState + 1) % states.Length);
    }

    public void PreviousState()
    {
        SwitchState((currentState - 1) % states.Length);
    }

    public void SetInitialState()
    {
        SwitchState(_initialState);
    }

    protected virtual void StateSwitchActions(int oldState, int newState)
    {

    }

    public void SyncNames()
    {
        stateNames = new string[states.Length];
        for (int i = 0; i < states.Length; i++)
            stateNames[i] = states[i].name;
    }

    public void ClampCurrentState()
    {
        currentState = Math.Clamp(currentState, 0, states.Length - 1);
    }

    public void UpdateState()
    {
        int value = _currentState;
        _currentState = prevState;
        SwitchState(value);
        prevState = value;
    }

    [Serializable]
    public struct State
    {
        public string name;
        public GameObject[] linkedObjects; //TO DO: Add ReverseLinkedObjects property. This should be another struct with the object and the bool inside.
        [FoldoutGroup("$EventsFoldout")]
        public DXEvent enter;
        [FoldoutGroup("$EventsFoldout")]
        public DXEvent exit;

        public State(string name)
        {
            this.name = name;
            linkedObjects = new GameObject[0];
            enter = null;
            exit = null;
        }

#if UNITY_EDITOR
        public string EventsFoldout()
        {
            return "Events" + ((enter.IsNull() && exit.IsNull()) ? "" : " ⚠");
        }
#endif
    }
}

/*
#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(StateMachine))]
public class StateMachine_Inspector : Editor
{
    StateMachine obj;
    SerializedProperty states;
    SerializedProperty stateNames;
    SerializedProperty currentState;
    ReorderableList statesList;

    const string relativeNameProperty = "name";
    const string relativeLinkedObjectsProperty = "linkedObjects";
    const string relativeStateEnterProperty = "enter";
    const string relativeStateExitProperty = "exit";

    void OnEnable()
    {
        obj = (StateMachine)target;
        states = serializedObject.FindProperty("states");
        stateNames = serializedObject.FindProperty("stateNames");
        currentState = serializedObject.FindProperty("_currentState");

        statesList = new ReorderableList(serializedObject, states, true, true, true, true)
        {
            elementHeightCallback = ElementHeightCallback,
            drawHeaderCallback = DrawListHeader,
            drawElementCallback = DrawElement,
            onSelectCallback = OnSelectElement,
            onReorderCallback = OnReorderList,
            onAddCallback = OnAddElement,
            onRemoveCallback = OnRemoveElement
        };

        SyncNames();
        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (obj.connectedStateMachine != null)
            if (GUILayout.Button("Update states from connected machine"))
                obj.UpdateStatesFromConnectedMachine();
        obj.currentState = StringPopupAttribute.IntPopup(obj.currentState, target,
            new StringPopupAttribute("stateNames"/*, "stringPairArray"*//*), "Current State",
            EditorGUILayout.GetControlRect(), currentState);
        statesList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    void SyncNames()
    {
        stateNames.arraySize = states.arraySize;
        for (int i = 0; i < stateNames.arraySize; i++)
        {
            SerializedProperty name = states.GetArrayElementAtIndex(i).FindPropertyRelative(relativeNameProperty);
            stateNames.GetArrayElementAtIndex(i).stringValue = string.IsNullOrEmpty(name.stringValue) ? i.ToString() : name.stringValue;
        }
    }

    #region ReorderableList callbacks
    void DrawListHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "States");
    }

    private float ElementHeightCallback(int index)
    {
        SerializedProperty state = statesList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty stateName = state.FindPropertyRelative(relativeNameProperty);
        SerializedProperty stateLinkedObjects = state.FindPropertyRelative(relativeLinkedObjectsProperty);
        SerializedProperty stateEnterEvent = state.FindPropertyRelative(relativeStateEnterProperty);
        SerializedProperty stateExitEvent = state.FindPropertyRelative(relativeStateExitProperty);

        //Gets the height of the element.
        float propertyHeight = EditorGUIUtility.singleLineHeight;
        if (state.isExpanded)
        {
            propertyHeight = EditorGUI.GetPropertyHeight(stateName) +
                EditorGUI.GetPropertyHeight(stateLinkedObjects, true);
            if (!stateEnterEvent.isExpanded) propertyHeight +=
                EditorGUI.GetPropertyHeight(stateEnterEvent, true) +
                EditorGUI.GetPropertyHeight(stateExitEvent, true) + 15f;
            else propertyHeight += EditorGUIUtility.singleLineHeight;
        }

        float spacing = EditorGUIUtility.singleLineHeight / 4;

        return propertyHeight + spacing;
    }

    void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty state = statesList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty stateName = state.FindPropertyRelative(relativeNameProperty);
        SerializedProperty stateLinkedObjects = state.FindPropertyRelative(relativeLinkedObjectsProperty);
        SerializedProperty stateEnterEvent = state.FindPropertyRelative(relativeStateEnterProperty);
        SerializedProperty stateExitEvent = state.FindPropertyRelative(relativeStateExitProperty);
        rect.y += 2;

        //We get the name property of our element so we can display this in our list
        string elementTitle = index + ". " + (string.IsNullOrEmpty(stateName.stringValue)
            ? "Unnamed" : stateName.stringValue);

        //Draw the list item as a property field, just like Unity does internally.
        float widthMult = 1f;
        float widthSum = -10f;
        if (state.isExpanded)
        {
            //Draw foldout
            state.isExpanded = EditorGUI.Foldout(
                new Rect(rect.x + 10, rect.y,
                (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                state.isExpanded, "");

            //Draw name
            EditorGUI.BeginChangeCheck();
            float y = rect.y;
            EditorGUI.PropertyField(
                new Rect(rect.x + 10, y,
                (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                stateName, new GUIContent("Name"));
            if (EditorGUI.EndChangeCheck()) SyncNames();
            y += EditorGUI.GetPropertyHeight(stateName) + 2;

            //Draw LinkedGameObjects
            EditorGUI.PropertyField(
                new Rect(rect.x + 10, y,
                (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                stateLinkedObjects, new GUIContent("Linked Objects"), true);
            y += EditorGUI.GetPropertyHeight(stateLinkedObjects);

            //Draw events foldout
            stateEnterEvent.isExpanded = !EditorGUI.Foldout(
                new Rect(rect.x + 10, y,
                (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                !stateEnterEvent.isExpanded, "Enter and Exit Events");
            y += EditorGUIUtility.singleLineHeight;

            //Draw events
            if (!stateEnterEvent.isExpanded)
            {
                EditorGUI.PropertyField(
                    new Rect(rect.x + 10, y,
                    (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                    stateEnterEvent, new GUIContent("Enter"), true);
                y += EditorGUI.GetPropertyHeight(stateEnterEvent);
                EditorGUI.PropertyField(
                    new Rect(rect.x + 10, y,
                    (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                    stateExitEvent, new GUIContent("Exit"), true);
            }
        }
        else
        {
            //Draw foldout
            state.isExpanded = EditorGUI.Foldout(
                new Rect(rect.x + 10, rect.y,
                (rect.width * widthMult) + widthSum, EditorGUIUtility.singleLineHeight),
                state.isExpanded, elementTitle);
        }
    }

    void OnSelectElement(ReorderableList list)
    {

    }

    void OnReorderList(ReorderableList list)
    {
        SyncNames();

        serializedObject.ApplyModifiedProperties();
        //SyncLocalNames();
    }

    void OnAddElement(ReorderableList list)
    {
        new ReorderableList.Defaults().DoAddButton(list);

        SyncNames();

        serializedObject.ApplyModifiedProperties();
    }

    void OnRemoveElement(ReorderableList list)
    {
        new ReorderableList.Defaults().DoRemoveButton(list);

        SyncNames();

        serializedObject.ApplyModifiedProperties();
    }
    #endregion
}
#endif
*/

#if PLAYMAKER
namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Croxel")]
    [Tooltip("Switches the StateMachine state")]
    public class SwitchStateMachineState : FsmStateAction
    {
        public StateMachine stateMachine;
        [StringPopup("names")]
        public int state;

        public override void Reset()
        {
            if (stateMachine == null)
                stateMachine = Owner.GetComponentInChildren<StateMachine>();
        }

        // Code that runs on entering the state.
        public override void OnEnter()
        {
            stateMachine.SwitchState(state);
        }
    }
}
#endif
