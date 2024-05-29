using UnityEngine;
using UnityEngine.Events;

public class DisableEvents : MonoBehaviour
{
    [SerializeField]
    DXEvent actions = null;

    void OnDisable()
    {
        actions?.Invoke();
    }
}
