using UnityEngine;

public class PeriodicEvent_Random : PeriodicEvent
{
    [SerializeField]
    [Min(0.02f)]
    Vector2 secondsRange = new Vector2(0.02f, 1f);
    [SerializeField]
    bool onlyRandomizeOnEnable = false;

    protected override void OnEnable()
    {
        seconds = Random.Range(secondsRange.x, secondsRange.y);
        base.OnEnable();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        seconds = secondsRange.x;
    }
#endif

    protected override void NewPeriod()
    {
        base.NewPeriod();
        if (!onlyRandomizeOnEnable)
            seconds = Random.Range(secondsRange.x, secondsRange.y);
    }
}
