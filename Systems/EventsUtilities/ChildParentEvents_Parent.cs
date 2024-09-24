using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

//TO DO: This could be combined with other children management scripts
//like the one that activates and deactivates children
//RavioliButton has a lot of similarities with this script and maybe it could inherit from it.
public class ChildParentEvents_Parent : MonoBehaviour
{
    public DXEvent[] parentEvents = null;

    List<ChildParentEvents_Child> childEvents;
    int currentChild = 0;

    bool init;

    void OnEnable()
    {
        SearchChildEvents();
    }

    void OnDisable()
    {
        childEvents.Clear();
        init = false;
    }

    void SearchChildEvents()
    {
        CreateListIfNull();
        if (!init)
        {
            ChildParentEvents_Child[] arr = GetComponentsInChildren<ChildParentEvents_Child>();
            for (int i = 0; i < arr.Length; i++)
                AddChild(arr[i]);
            init = true;
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

            SearchChildEvents();
            if (childEvents != null)
                foreach (ChildParentEvents_Child childEvent in childEvents)
                    if (IsChildEventAvailable(childEvent, index))
                        childEvent.childEvents[index]?.Invoke();
        }
    }

    public void CallAllChildEvents()
    {
        if (this.IsActiveAndEnabled())
        {
            SearchChildEvents();
            if (childEvents != null)
                foreach (ChildParentEvents_Child childEvent in childEvents)
                    if (AreChildEventsAvailable(childEvent))
                        foreach (DXEvent e in childEvent.childEvents)
                            e?.Invoke();
        }
    }

    public void CallEventOnCurrentChild(int index)
    {
        if (this.IsActiveAndEnabled())
        {
            SearchChildEvents();
            if (childEvents != null)
            {
                ChildParentEvents_Child childEvent = childEvents[currentChild];
                if (IsChildEventAvailable(childEvent, index))
                    childEvent.childEvents[index]?.Invoke();
            }
        }
    }

    public void SetChild(int index)
    {
        SearchChildEvents();
        currentChild = Mathf.Clamp(index, 0, childEvents.Count - 1);
    }

    public void SetRandomChild()
    {
        SearchChildEvents();
        currentChild = Random.Range(0, childEvents.Count);
    }

    public void AddChild(ChildParentEvents_Child child)
    {
        CreateListIfNull();
        if (!childEvents.Contains(child))
            childEvents.Add(child);
    }

    public void RemoveChild(ChildParentEvents_Child child)
    {
        if ((childEvents != null) && childEvents.Contains(child))
            childEvents.Remove(child);
    }

    void CreateListIfNull()
    {
        if (childEvents == null) childEvents = new List<ChildParentEvents_Child>();
    }

    bool AreChildEventsAvailable(ChildParentEvents_Child childEvents)
    {
        return childEvents.IsActiveAndEnabled() && (childEvents.childEvents != null);
    }

    bool IsChildEventAvailable(ChildParentEvents_Child childEvents, int index)
    {
        return AreChildEventsAvailable(childEvents) && (index < childEvents.childEvents.Length);
    }
}
