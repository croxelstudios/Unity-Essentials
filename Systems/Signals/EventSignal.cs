using UnityEngine;
using QFSW.QC;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Croxel Scriptables/EventSignal")]
public class EventSignal : BaseSignal
{
#if PLAYMAKER
    [SerializeField]
    [Tooltip("Filtering by tag won't work with this")]
    string playMakerBroadcast = "";

    void OnValidate()
    {
        if (playMakerBroadcast == "")
            playMakerBroadcast = "SIGNAL_" + name.Replace("Global_", "");
    }
#endif

    [TagSelector]
    public void CallSignal(string tag = "") //Change type here
    {
        beforeCall?.Invoke();
        if (dynamicSearch) DynamicSearch<EventsManager>(); //Change type here
        if (listeners != null)
        {
            for (int i = (listeners.Count - 1); i >= 0; i--)
            {
                if ((tag == "") || (tag == listeners[i].receiver.tag))
                    ((EventsManager)listeners[i].receiver). //Change type here
                        LaunchActions(listeners[i].index); //Change type here
            }
        }
        if (dynamicSearch) listeners = null;
        called?.Invoke();
#if PLAYMAKER
        PlayMakerFSM.BroadcastEvent(playMakerBroadcast);
#endif
    }

    public void CallSignal(IEnumerable<GameObject> objects) //Change type here
    {
        CallSignal<GameObject>(objects);
    }

    public void CallSignal(IEnumerable<Transform> transforms) //Change type here
    {
        CallSignal<Transform>(transforms);
    }

    void CallSignal<T>(IEnumerable<T> objects) where T : Object //Change type here
    {
        beforeCall?.Invoke();
        if (dynamicSearch) DynamicSearch<EventsManager>(); //Change type here
        if (listeners != null)
        {
            for (int i = (listeners.Count - 1); i >= 0; i--)
            {
                bool isChild = false;
                foreach (T obj in objects)
                {
                    Transform tr = obj.GetTransform();
                    if (listeners[i].receiver.transform.IsChildOf(tr))
                    {
                        isChild = true;
                        break;
                    }
                }
                if (isChild)
                    ((EventsManager)listeners[i].receiver). //Change type here
                        LaunchActions(listeners[i].index); //Change type here
            }
        }
        if (dynamicSearch) listeners = null;
        called?.Invoke();
#if PLAYMAKER
        PlayMakerFSM.BroadcastEvent(playMakerBroadcast);
#endif
    }

    public void CallSignal() //Change type here
    {
        CallSignal(""); //Change type here
    }

    [Sirenix.OdinInspector.Button("Call Signal")]
    void CallSignalOnCurrentTag()
    {
        CallSignal(currentTag); //Change type here
    }

    [Command("call-signal")]
    public static void CallSignal(EventSignal signal) //Change type here
    {
        signal.CallSignal();
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EventSignal))] //Change type here
public class EventSignalPropertyDrawer : BaseSignalPropertyDrawer //Change type here
{
}
#endif

#if PLAYMAKER
namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Croxel")]
    [Tooltip("Calls a signal")]
    public class PMCallEventSignal : FsmStateAction
    {
        public EventSignal signal;

        // Code that runs on entering the state.
        public override void OnEnter()
        {
            signal.CallSignal();
            Finish();
        }
    }
}
#endif
