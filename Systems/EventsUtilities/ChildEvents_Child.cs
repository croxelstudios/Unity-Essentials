using UnityEngine;

public class ChildEvents_Child : MonoBehaviour
{
    //TO DO: Filter by a tagname or something because we could be using multiple of these in the same object
    //TO DO: EventsNames support?
    public DXEvent[] events = null;

    void OnDisable()
    {
        
    }
}
