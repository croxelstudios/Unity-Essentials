using UnityEngine;

public class RegisterCounter : MonoBehaviour
{
    [SerializeField]
    StringList register = null;
    [SerializeField]
    bool launchOnEnable = true;
    [SerializeField]
    DXIntEvent onRegisterChanged = null;

    void OnEnable()
    {
        if (launchOnEnable)
            Launch();
        register.valueRemoved.AddListener(Launch);
        register.valueAdded.AddListener(Launch);
    }

    void OnDisable()
    {
        register.valueRemoved.RemoveListener(Launch);
        register.valueAdded.RemoveListener(Launch);
    }

    void Launch(string foo)
    {
        Launch();
    }

    public void Launch()
    {
        if (register != null)
            onRegisterChanged?.Invoke(register.Count);
    }
}
