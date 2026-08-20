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
    [StringSelector("stateNames")]
    [OnValueChanged("UpdateState")]
    int _currentState = 0;

    int _initialState;

    private void Awake()
    {
        _initialState = _currentState;
    }

    protected virtual void OnEnable()
    {
        if (launchOnEnable)
            EnterStateAction();
    }

    public int currentState
    {
        get { return _currentState; }
        set
        {
            if ((_currentState != value) && (value < states.Length) && (value >= 0))
                    SwitchState(value);
        }
    }

    [OnValueChanged("SyncNames", true)]
    [OnValueChanged("ClampCurrentState")]
    public State[] states = null;

    [HideInInspector]
    public string[] stateNames;
    int prevState = 0;
    [HideInInspector]
    public string[] eventNames;

#if UNITY_EDITOR
    StateMachine prevConnectedSM;
    bool wasprevStateUpdated = false;

    [ShowIf("@connectedStateMachine != null")]
    [PropertyOrder(-1)]
    [Button]
    public void UpdateStatesFromConnectedMachine()
    {
        states = states.Resize(connectedStateMachine.states.Length);
        stateNames = stateNames.Resize(states.Length);
        for (int i = 0; i < states.Length; i++)
        {
            states[i].name = connectedStateMachine.states[i].name;
            stateNames[i] = states[i].name;
        }
    }

    PerFrameTracker tracker;

    public virtual void OnValidate()
    {
        tracker = tracker.CreateIfNull();
        if (!tracker.Simple()) return;

        if (states.IsNullOrEmpty())
        {
            states = new State[1];
            states[0].name = "Default";
            stateNames = new string[1] { states[0].name };
        }

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
                connectedStateMachine.states = new State[0];
            for (int i = 0; i < states.Length; i++)
            {
                if (i < connectedStateMachine.states.Length)
                    newStates[i] = connectedStateMachine.states[i];
                newStates[i].name = states[i].name;
            }
            connectedStateMachine.states = newStates;
            connectedStateMachine.OnValidate();
        }
    }

    void RecordGameObjectModificationsFromPrefab()
    {
        EditorUtility.SetDirty(gameObject);
        Component[] components = GetComponentsInChildren<Component>();
        foreach (Component component in components)
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
    }
#endif

    [StringSelector("stateNames")]
    public virtual void SwitchState(int newState)
    {
        newState = Mathf.Clamp(newState, 0, states.Length - 1);
        if (_currentState != newState)
        {
            ExitStateAction();
            StateSwitchActions(_currentState, newState);
            _currentState = newState;
            if (connectedStateMachine != null)
                connectedStateMachine.SwitchState(newState);
            EnterStateAction();
#if UNITY_EDITOR
            if (!Application.isPlaying)
                RecordGameObjectModificationsFromPrefab();
#endif
        }
    }

    [StringSelector("eventNames")]
    public void LaunchEvent(string name)
    {
        if (this.IsActiveAndEnabled())
        {
            int index = Array.IndexOf(eventNames, name);
            states[currentState].events[index]?.Invoke();
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
        if (states.IsNullOrEmpty()) currentState = 0;
        else currentState = Math.Clamp(currentState, 0, states.Length - 1);
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
        public GameObject[] linkedObjects;
        //TO DO: Add bool array property to reverse objects activation.
        //This should appear as a struct with the object and the bool
        //inside with some trick Attribute similar to NamedList.
        [FoldoutGroup("@FoldoutName(\"Enter Exit Events\")")]
        public DXEvent enter;
        [FoldoutGroup("@FoldoutName(\"Enter Exit Events\")")]
        public DXEvent exit;
        [FoldoutGroup("@FoldoutName(\"State-dependant Events\", events)")]
        [NamedList("/.eventNames", false, true)]
        [SerializeField] public byte _foo;
        [HideInInspector]
        public DXEvent[] events;

        public State(string name, StateMachine parentMachine)
        {
            this.name = name;
            linkedObjects = new GameObject[0];
            enter = null;
            exit = null;
            events = null;
            _foo = 0;
        }

#if UNITY_EDITOR
        public string FoldoutName(string name)
        {
            return name.ContentMarker(enter, exit);
        }

        public string FoldoutName(string name, params object[] objects)
        {
            return name.ContentMarker(objects);
        }
#endif
    }
}

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
