using UnityEngine;

public static class MonoBehaviourExtension_IsActiveAndEnabled
{
    public static bool IsActiveAndEnabled(this MonoBehaviour behaviour, bool deactivationDelay = true)
    {
        bool current = behaviour.enabled && behaviour.gameObject.activeInHierarchy;
        if (deactivationDelay) return behaviour.isActiveAndEnabled || current;
        else return current;
    }
}
