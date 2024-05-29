using UnityEngine;

public class BBaseSignalCaller : MonoBehaviour
{
    [SerializeField]
    [TagSelector]
    protected string _tag = "";
    [SerializeField]
    protected bool checkActiveState = true; //TO DO: Maybe should be an array

    void OnDisable()
    {
    }

    public void ResetAllSignals()
    {
        BaseSignal.ResetAllSignals();
    }
}
