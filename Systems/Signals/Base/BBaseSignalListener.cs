using UnityEngine;

public class BBaseSignalListener : MonoBehaviour
{
    [HideInInspector]
    public BaseSignal[] signals = null;
    [SerializeField]
    [Tooltip("Disabling this only takes effect when the signal is dynamic")]
    protected bool checkActiveState = true;
    [SerializeField]
    public int priority = 0;
    public bool CheckActiveState { get { return checkActiveState; } set { checkActiveState = value; } }

    protected virtual void OnEnable()
    {
        UpdateSignals();
        for (int i = 0; i < signals.Length; i++)
            if ((signals[i] != null) && (!signals[i].dynamicSearch))
            {
                signals[i].OnEnable();
                signals[i].AddAction(this, i);
            }
    }

    protected virtual void OnDisable()
    {
        for (int i = 0; i < signals.Length; i++)
            if ((signals[i] != null) && (!signals[i].dynamicSearch))
                signals[i].RemoveAction(this, i);
    }

    public virtual void UpdateSignals()
    {
    }
}

public class BBaseSignalListener<T> : BBaseSignalListener
{
    [SerializeField]
    bool launchOnEnable = true;
    public bool launchOnEnable_ { get { return launchOnEnable; } set { value = launchOnEnable; } }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (launchOnEnable) LaunchActions_OnEnable();
    }

    protected virtual void LaunchActions_OnEnable()
    {
        LaunchActions();
    }

    public virtual void LaunchActions()
    {
        Debug.LogError("LaunchActions not implemented for this Listener");
    }

    public virtual void LaunchActions(int index, T value)
    {
        Debug.LogError("LaunchActions not implemented for this Listener");
    }
}
