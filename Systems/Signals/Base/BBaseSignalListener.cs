using UnityEngine;

public class BBaseSignalListener : MonoBehaviour
{
    [HideInInspector]
    public BaseSignal[] signals = null;
    [SerializeField]
    [Tooltip("Disabling this only takes effect when the signal is dynamic")]
    protected bool checkActiveState = true;
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
