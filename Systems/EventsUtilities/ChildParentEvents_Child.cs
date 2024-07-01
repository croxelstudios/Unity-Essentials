using UnityEngine;

public class ChildParentEvents_Child : MonoBehaviour
{
    //TO DO: Filter by a tagname or something because we could be using multiple of these in the same object
    //TO DO: EventsNames support?
    public DXEvent[] childEvents = null;

    ChildParentEvents_Parent[] parents;

    void OnEnable()
    {
        GetParents();
        for (int i = 0; i < parents.Length; i++)
            parents[i].AddChild(this);
    }

    void OnDisable()
    {
        for (int i = 0; i < parents.Length; i++)
            parents[i].RemoveChild(this);
    }

    void GetParents()
    {
        if (parents == null)
            parents = GetComponentsInParent<ChildParentEvents_Parent>();
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

            GetParents();
            foreach (ChildParentEvents_Parent parent in parents)
                if (parent.IsActiveAndEnabled() && (parent.parentEvents != null) && (index < parent.parentEvents.Length))
                    parent.parentEvents[index]?.Invoke();
        }
    }

    public void CallAllParentEvents()
    {
        if (this.IsActiveAndEnabled())
        {
            GetParents();
            foreach (ChildParentEvents_Parent parent in parents)
                if (parent.IsActiveAndEnabled() && (parent.parentEvents != null))
                    foreach (DXEvent e in parent.parentEvents)
                        e?.Invoke();
        }
    }
}
