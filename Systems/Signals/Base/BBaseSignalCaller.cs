using UnityEngine;

public class BBaseSignalCaller : MonoBehaviour
{
    [SerializeField]
    [TagSelector]
    protected string _tag = "";
    [SerializeField]
    protected bool checkActiveState = true;

    void OnDisable()
    {
    }

    public void ResetAllSignals()
    {
        BaseSignal.ResetAllSignals();
    }
}
