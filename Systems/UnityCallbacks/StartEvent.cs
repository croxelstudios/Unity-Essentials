using UnityEngine;
using UnityEngine.Events;

public class StartEvent : MonoBehaviour
{
    [SerializeField]
    DXEvent actions = null;
    [SerializeField]
    bool launchOnEnable = false;
    [SerializeField]
    int repetitions = 1;

    void Start()
    {
        if (this.IsActiveAndEnabled() && !launchOnEnable) Launch();
    }

    void OnEnable()
    {
        if (launchOnEnable) Launch();
    }

    public void Launch()
    {
        for (int i = 0; i < repetitions; i++)
            actions?.Invoke();
    }
}
