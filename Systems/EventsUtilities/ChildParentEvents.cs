using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Linq;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChildParentEvents : MonoBehaviour
{
    public bool propagateToChildren = false;
    public bool propagateToParents = false;
    public bool ignoreFromChildren = false;
    public bool ignoreFromParents = false;
    [ListDrawerSettings(ShowFoldout = false)]
    //TO DO: Doesn't work because I'm using GenericNamedEvents_Inspector.
    //The GenericNamedEvents_Inspector class should be remade into an Attribute.
    public string[] notedTargetEventNames = null;
    [HideInInspector]
    public DXEvent[] events = null;
    [HideInInspector]
    public string[] eventNames = null;

    List<ChildParentEvents> parentEvents;
    [HideInInspector]
    public List<ChildParentEvents> parentEventsISubscribedAsChild;
    List<ChildParentEvents> childEvents;
    [HideInInspector]
    public List<ChildParentEvents> childrenEventsISubscribedAsParent;
    int currentChild = 0;

    static List<ChildParentEvents> auxEvents;

    bool init;

    void OnEnable()
    {
        if (!init)
        {
            SearchParentEvents();
            SearchChildEvents();
            SortChildren();
            init = true;
        }
    }

    void OnDisable()
    {
        RemoveFromParentEvents();
        parentEvents.SmartClear();
        RemoveFromChildrenEvents();
        childEvents.SmartClear();
        init = false;
    }

    #region Children Functions
    [StringPopup("notedTargetEventNames")]
    public void CallChildEvent(string name)
    {
        if (this.IsActiveAndEnabled())
        {
            OnEnable();
            auxEvents = auxEvents.ClearOrCreate();
            auxEvents.TryAddRange(childEvents);
            if (!auxEvents.IsNullOrEmpty())
                for (int i = auxEvents.Count - 1; i >= 0; i--)
                    if ((i < auxEvents.Count) && IsChildEventAvailable(auxEvents[i], name))
                    {
                        IEnumerable<int> indices = auxEvents[i].eventNames.IndicesOf(name);
                        foreach (int index in indices)
                            auxEvents[i].events[index]?.Invoke();
                    }
        }
    }

    public void CallChildEvent(int index)
    {
        if (this.IsActiveAndEnabled())
        {
            if (index < 0)
            {
                Debug.LogError("You tried to call an event on a negative index.");
                return;
            }

            OnEnable();
            auxEvents = auxEvents.ClearOrCreate();
            auxEvents.TryAddRange(childEvents);
            if (!auxEvents.IsNullOrEmpty())
                for (int i = auxEvents.Count - 1; i >= 0; i--)
                    if ((i < auxEvents.Count) && IsChildEventAvailable(auxEvents[i], index))
                        auxEvents[i].events[index]?.Invoke();
        }
    }

    public void CallAllChildEvents()
    {
        if (this.IsActiveAndEnabled())
        {
            OnEnable();
            auxEvents = auxEvents.ClearOrCreate();
            auxEvents.TryAddRange(childEvents);
            if (!auxEvents.IsNullOrEmpty())
                for (int i = auxEvents.Count - 1; i >= 0; i--)
                    if ((i < auxEvents.Count) && AreChildEventsAvailable(auxEvents[i]))
                        foreach (DXEvent e in auxEvents[i].events)
                            e?.Invoke();
        }
    }

    public void CallEventOnCurrentChild(int index)
    {
        if (this.IsActiveAndEnabled())
        {
            OnEnable();
            if (!childEvents.IsNullOrEmpty())
            {
                ChildParentEvents childEvent = childEvents[currentChild];
                if (IsChildEventAvailable(childEvent, index))
                    childEvent.events[index]?.Invoke();
            }
        }
    }

    public void CallEventOnCurrentChild(string name)
    {
        if (this.IsActiveAndEnabled())
        {
            OnEnable();
            if (!childEvents.IsNullOrEmpty())
            {
                ChildParentEvents childEvent = childEvents[currentChild];
                if (IsChildEventAvailable(childEvent, name))
                {
                    IEnumerable<int> indices = childEvents[currentChild].eventNames.IndicesOf(name);
                    foreach (int index in indices)
                        childEvents[currentChild].events[index]?.Invoke();
                }
            }
        }
    }

    public void SetChild(int index)
    {
        currentChild = Mathf.Clamp(index, 0, childEvents.Count - 1);
    }

    public void SetRandomChild()
    {
        currentChild = Random.Range(0, childEvents.Count);
    }

    bool AreChildEventsAvailable(ChildParentEvents childEvents)
    {
        return childEvents.IsActiveAndEnabled() && (!childEvents.ignoreFromParents) &&
            (!childEvents.events.IsNullOrEmpty());
    }

    bool IsChildEventAvailable(ChildParentEvents childEvents, int index)
    {
        return AreChildEventsAvailable(childEvents) && (index < childEvents.events.Length);
    }

    bool IsChildEventAvailable(ChildParentEvents childEvents, string name)
    {
        return AreChildEventsAvailable(childEvents) && childEvents.eventNames.Contains(name);
    }

    void SortChildren()
    {
        if (!childEvents.IsNullOrEmpty())
            childEvents.Sort(delegate (ChildParentEvents x, ChildParentEvents y)
            {
                return x.transform.ChildIndex().CompareTo(y.transform.ChildIndex());
            });
    }
    #endregion

    #region Parents Functions
    [StringPopup("notedTargetEventNames")]
    public void CallParentEvent(string name)
    {
        if (this.IsActiveAndEnabled())
        {
            OnEnable();
            auxEvents = auxEvents.ClearOrCreate();
            auxEvents.TryAddRange(parentEvents);
            if (!auxEvents.IsNullOrEmpty())
                for (int i = auxEvents.Count - 1; i >= 0; i--)
                    if ((i < auxEvents.Count) && IsParentEventAvailable(auxEvents[i], name))
                    {
                        IEnumerable<int> indices = auxEvents[i].eventNames.IndicesOf(name);
                        foreach (int index in indices)
                            auxEvents[i].events[index]?.Invoke();
                    }
        }
    }

    public void CallParentEvent(int index)
    {
        if (this.IsActiveAndEnabled())
        {
            if (index < 0)
            {
                Debug.LogError("You tried to call an event on a negative index.");
                return;
            }

            OnEnable();
            auxEvents = auxEvents.ClearOrCreate();
            auxEvents.TryAddRange(parentEvents);
            if (!auxEvents.IsNullOrEmpty())
                for (int i = auxEvents.Count - 1; i >= 0; i--)
                    if ((i < auxEvents.Count) && IsParentEventAvailable(auxEvents[i], index))
                        auxEvents[i].events[index]?.Invoke();
        }
    }

    public void CallAllParentEvents()
    {
        if (this.IsActiveAndEnabled())
        {
            OnEnable();
            auxEvents = auxEvents.ClearOrCreate();
            auxEvents.TryAddRange(parentEvents);
            if (!auxEvents.IsNullOrEmpty())
                for (int i = auxEvents.Count - 1; i >= 0; i--)
                    if ((i < auxEvents.Count) && AreParentEventsAvailable(auxEvents[i]))
                        foreach (DXEvent e in auxEvents[i].events)
                            e?.Invoke();
        }
    }

    bool AreParentEventsAvailable(ChildParentEvents parentEvent)
    {
        return parentEvent.IsActiveAndEnabled() && (!parentEvent.ignoreFromChildren) &&
            (!parentEvent.events.IsNullOrEmpty());
    }

    bool IsParentEventAvailable(ChildParentEvents parentEvent, int index)
    {
        return AreParentEventsAvailable(parentEvent) && (index < parentEvent.events.Length);
    }

    bool IsParentEventAvailable(ChildParentEvents parentEvent, string name)
    {
        return AreParentEventsAvailable(parentEvent) && parentEvent.eventNames.Contains(name);
    }
    #endregion

    #region Add and remove CHILDREN
    void SearchChildEvents()
    {
        ChildParentEvents[] arr = GetComponentsInChildren<ChildParentEvents>();
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i].TryAddParent(this);
            TryAddChild(arr[i]);
        }
    }

    public bool TryAddChild(ChildParentEvents newChild)
    {
        //If it's the same object, return
        if ((newChild == this) || childEvents.NotNullContains(newChild))
            return false;

        //Check if present children allow this child
        childEvents = childEvents.CreateIfNull();
        foreach (ChildParentEvents child in childEvents)
            if ((!child.propagateToChildren) && newChild.transform.IsChildOf(child.transform))
                return false;

        //Remove present children not allowed by the new child
        if (!newChild.propagateToChildren)
            for (int i = childEvents.Count - 1; i >= 0; i--)
            {
                ChildParentEvents child = childEvents[i];
                if (child.transform.IsChildOf(newChild.transform))
                    RemoveChild(child);
            }

        childEvents = childEvents.CreateAdd(newChild);
        newChild.parentEventsISubscribedAsChild = newChild.parentEventsISubscribedAsChild.CreateAdd(this);
        return true;
    }

    void RemoveFromChildrenEvents()
    {
        for (int i = 0; i < childrenEventsISubscribedAsParent.Count; i++)
            childrenEventsISubscribedAsParent[i].RemoveParent(this);
        childrenEventsISubscribedAsParent.SmartClear();
    }

    public void RemoveChild(ChildParentEvents child)
    {
        childEvents.SmartRemove(child);
    }
    #endregion

    #region Add and remove PARENTS
    void SearchParentEvents()
    {
        ChildParentEvents[] arr = GetComponentsInParent<ChildParentEvents>();
        for (int i = 0; i < arr.Length; i++)
        {
            TryAddParent(arr[i]);
            arr[i].TryAddChild(this);
        }
    }

    public bool TryAddParent(ChildParentEvents newParent)
    {
        //If it's the same object, return
        if ((newParent == this) || parentEvents.NotNullContains(newParent))
            return false;

        //Check if present parents allow this parent
        parentEvents = parentEvents.CreateIfNull();
        foreach (ChildParentEvents parent in parentEvents)
            if ((!parent.propagateToParents) && parent.transform.IsChildOf(newParent.transform))
                return false;

        //Remove present parents not allowed by the new parent
        if (!newParent.propagateToParents)
            for (int i = parentEvents.Count - 1; i >= 0; i--)
            {
                ChildParentEvents parent = parentEvents[i];
                if (newParent.transform.IsChildOf(parent.transform))
                    RemoveParent(parent);
            }

        parentEvents = parentEvents.CreateAdd(newParent);
        newParent.childrenEventsISubscribedAsParent = newParent.childrenEventsISubscribedAsParent.CreateAdd(this);
        return true;
    }

    void RemoveFromParentEvents()
    {
        for (int i = 0; i < parentEventsISubscribedAsChild.Count; i++)
            parentEventsISubscribedAsChild[i].RemoveChild(this);
        parentEventsISubscribedAsChild.SmartClear();
    }

    public void RemoveParent(ChildParentEvents parent)
    {
        parentEvents.SmartRemove(parent);
    }
    #endregion

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
        }
    }

    public void SyncNames(bool priorizeLocal = false)
    {
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ChildParentEvents))]
public class ChildParentEvents_Inspector : GenericNamedEvents_Inspector
{
    protected override void NameArrayChanged(bool priorizeLocal = true)
    {
        base.NameArrayChanged();
        ((ChildParentEvents)target).SyncNames(priorizeLocal);
    }
}
#endif
