using UnityEditor;
using UnityEngine;
using System.Linq;

[ExecuteAlways]
public class StateMachine_ByChildren : StateMachine
{
#if UNITY_EDITOR
    public override void OnValidate()
    {
        base.OnValidate();
        Update();
    }

    void Update()
    {
        if ((!Application.isPlaying) && DidChildrenChange())
        {
            if (states == null) states = new State[0];
            State[] newStates = new State[transform.childCount];
            for (int i = 0; i < Mathf.Min(states.Length, newStates.Length); i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                newStates[i] = states[i];
                newStates[i].name = child.name;
                if (states[i].linkedObjects == null) newStates[i].linkedObjects = new GameObject[0];
                else newStates[i].linkedObjects = states[i].linkedObjects;
            }
            if (states.Length < transform.childCount)
                for (int i = states.Length; i < transform.childCount; i++)
                {
                    GameObject child = transform.GetChild(i).gameObject;
                    newStates[i] = new State(child.name);
                }
            states = newStates;
        }
    }

    bool DidChildrenChange()
    {
        if ((states == null) || (states.Length != transform.childCount)) return true;
        else
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (child.name != states[i].name) return true;
            }
            return false;
        }
    }

#endif

    protected override void StateSwitchActions(int oldState, int newState)
    {
        transform.GetChild(oldState).gameObject.SetActive(false);
        transform.GetChild(newState).gameObject.SetActive(true);
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(StateMachine_ByChildren))]
public class StateMachine_ByChildren_Inspector : StateMachine_Inspector
{
}
#endif
