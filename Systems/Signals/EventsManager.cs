using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Random = UnityEngine.Random;

public class EventsManager : BBaseSignalListener
{
    //string[] eventNames { get { return signalActions.Select(i => i.name).ToArray(); } }
    //[ChangeCheck("UpdateSignals")]
    [HideInInspector]
    public Action[] events = null;
#if UNITY_EDITOR
    [SerializeField]
    [HideInInspector]
    string[] eventNames = null;
#endif

    public override void UpdateSignals()
    {
        if ((signals == null) || (signals.Length == 0))
        {
            List<BaseSignal> sigs = new List<BaseSignal>();
            for (int i = 0; i < events.Length; i++)
                sigs.Add(events[i].signal);
            signals = sigs.ToArray();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (events != null)
        {
            if (eventNames == null) eventNames = new string[events.Length];
            if (eventNames.Length != events.Length)
            {
                string[] newNames = new string[events.Length];
                for (int i = 0; i < Mathf.Min(eventNames.Length, newNames.Length); i++)
                    newNames[i] = eventNames[i];
                eventNames = newNames;
            }
            for (int i = 0; i < events.Length; i++)
                if (events[i].signal != null) eventNames[i] = "(S) " + events[i].signal.name;
                else if (!string.IsNullOrEmpty(eventNames[i]) && eventNames[i].Contains("(S) ")) eventNames[i] = eventNames[i].Replace("(S) ", "S? ");
        }
    }
#endif

    [StringPopup("eventNames")]
    public void LaunchActions(int index) //Change type here
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            events[index].actions?.Invoke(); //Change type here
    }

    [ContextMenu("LaunchRandomAction")]
    public void LaunchRandomAction()
    {
        if (this.IsActiveAndEnabled() || !checkActiveState)
            events[Random.Range(0, events.Length)].actions?.Invoke(); //Change type here
    }

    public void LaunchRandomActionNTimes(int times)
    {
        for (int i = 0; i < times; i++)
            LaunchRandomAction();
    }

#if UNITY_EDITOR
    [ContextMenu("Create new EventSignal")] //Change type here
    void CreateEventSignal()
    {
        ScriptableObjectUtils.CreateScriptableObjectAsset<EventSignal>("EventSignal", "New EventSignal"); //Change type here (3)
    }
#endif

    [Serializable]
    public struct Action
    {
        public EventSignal signal; //Change type here
        public DXEvent actions; //Change type here

        public Action(EventSignal signal, DXEvent actions) //Change type here (2)
        {
            this.signal = signal;
            this.actions = actions;
        }
    }

    public void SyncNames(bool priorizeLocal = false)
    {
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EventsManager))]
public class EventsManager_Inspector : GenericNamedEvents_Inspector
{
    protected override void NameArrayChanged(bool priorizeLocal = true)
    {
        base.NameArrayChanged();
        ((EventsManager)target).SyncNames(priorizeLocal);
    }
}
#endif
