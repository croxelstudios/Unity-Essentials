using UnityEngine;

public static class BehaviourExtension_IsActiveAndEnabled
{
    public static bool IsActiveAndEnabled(this Behaviour behaviour, bool deactivationDelay = true)
    {
        if (behaviour == null)
            return false;

        bool current = behaviour.enabled && behaviour.gameObject.activeInHierarchy;
        if (deactivationDelay) return behaviour.isActiveAndEnabled || current;
        else return current;
    }
}
