using UnityEngine;
using Random = UnityEngine.Random;

public class ChildEvents_Parent : MonoBehaviour
{
    [SerializeField]
    bool searchEverytime = false;

    ChildEvents_Child[] childEvents;
    int currentChild = 0;

    void SearchChildEvents()
    {
        childEvents = GetComponentsInChildren<ChildEvents_Child>(true);
    }

    void Awake()
    {
        if (!searchEverytime) SearchChildEvents();
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

            if (searchEverytime || (childEvents == null)) SearchChildEvents();
            foreach (ChildEvents_Child childEvent in childEvents)
                if (childEvent.IsActiveAndEnabled() && (childEvent.events != null) && (index < childEvent.events.Length))
                    childEvent.events[index]?.Invoke();
        }
    }

    public void CallAllChildEvents()
    {
        if (this.IsActiveAndEnabled())
        {
            if (searchEverytime) SearchChildEvents();
            foreach (ChildEvents_Child childEvent in childEvents)
                if (childEvent.IsActiveAndEnabled() && (childEvent.events != null))
                    foreach (DXEvent e in childEvent.events)
                        e?.Invoke();
        }
    }

    public void CallEventOnCurrentChild(int index)
    {
        ChildEvents_Child childEvent = childEvents[currentChild];
        if (childEvent.IsActiveAndEnabled() && (childEvent.events != null) && (index < childEvent.events.Length))
            childEvent.events[index]?.Invoke();
    }

    public void SetChild(int index)
    {
        currentChild = Mathf.Clamp(index, 0, childEvents.Length - 1);
    }

    public void SetRandomChild()
    {
        currentChild = Random.Range(0, childEvents.Length);
    }
}
