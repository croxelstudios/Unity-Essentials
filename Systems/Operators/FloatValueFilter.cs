using UnityEngine;
using UnityEngine.Events;
using System;

public class FloatValueFilter : MonoBehaviour
{
    [SerializeField]
    Vector2 range = new Vector2(0f, 1f);
    [SerializeField]
    ButtonEvents events = new ButtonEvents();
    [SerializeField]
    DXFloatEvent filteredValue = null;

    bool state;

    public void FilterValue(int value)
    {
        FilterValue((float)value);
    }

    public void FilterValue(float value)
    {
        if ((value >= Mathf.Min(range.x, range.y)) && (value < Mathf.Max(range.x, range.y)))
        {
            filteredValue?.Invoke(value);
            if (!state)
            {
                events.Pressed?.Invoke();
                state = true;
            }
        }
        else if (state)
        {
            events.Released?.Invoke();
            state = false;
        }
    }

    [Serializable]
    struct ButtonEvents
    {
        public DXEvent Pressed;
        public DXEvent Released;

        public ButtonEvents(DXEvent pressed, DXEvent released)
        {
            Pressed = pressed;
            Released = released;
        }
    }
}
