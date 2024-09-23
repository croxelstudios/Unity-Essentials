using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

//TO DO: This could be combined with other children management scripts
//like the one that activates and deactivates children
public class ChildParentEvents_Parent : MonoBehaviour
{
    public DXEvent[] parentEvents = null;

    List<ChildParentEvents_Child> childEvents;
    int currentChild = 0;

    void SearchChildEvents()
    {
        CreateListIfNull();
        childEvents.Clear();
        childEvents.AddRange(GetComponentsInChildren<ChildParentEvents_Child>(true));
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

            if (childEvents != null)
                foreach (ChildParentEvents_Child childEvent in childEvents)
                    if (childEvent.IsActiveAndEnabled() && (childEvent.childEvents != null) && (index < childEvent.childEvents.Length))
                        childEvent.childEvents[index]?.Invoke();
        }
    }

    public void CallAllChildEvents()
    {
        if (this.IsActiveAndEnabled())
        {
            if (childEvents != null)
                foreach (ChildParentEvents_Child childEvent in childEvents)
                    if (childEvent.IsActiveAndEnabled() && (childEvent.childEvents != null))
                        foreach (DXEvent e in childEvent.childEvents)
                            e?.Invoke();
        }
    }

    public void CallEventOnCurrentChild(int index)
    {
        SearchChildEvents();
        ChildParentEvents_Child childEvent = childEvents[currentChild];
        if (childEvent.IsActiveAndEnabled() && (childEvent.childEvents != null) && (index < childEvent.childEvents.Length))
            childEvent.childEvents[index]?.Invoke();
    }

    public void SetChild(int index)
    {
        SearchChildEvents();
        currentChild = Mathf.Clamp(index, 0, childEvents.Count - 1);
    }

    public void SetRandomChild()
    {
        SearchChildEvents();
        List<ChildParentEvents_Child> active = new List<ChildParentEvents_Child>();
        for (int i = 0; i < childEvents.Count; i++)
            if (childEvents[i].IsActiveAndEnabled())
                active.Add(childEvents[i]);
        currentChild = Random.Range(0, active.Count);
        currentChild = childEvents.FindIndex(x => active[currentChild] == x);
    }

    public void AddChild(ChildParentEvents_Child child)
    {
        SearchChildEvents();
        if (!childEvents.Contains(child))
            childEvents.Add(child);
    }

    public void RemoveChild(ChildParentEvents_Child child)
    {
        SearchChildEvents();
        if (childEvents.Contains(child))
            childEvents.Remove(child);
    }

    void CreateListIfNull()
    {
        if (childEvents == null) childEvents = new List<ChildParentEvents_Child>();
    }
}
