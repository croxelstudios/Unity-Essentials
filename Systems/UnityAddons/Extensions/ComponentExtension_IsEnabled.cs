using UnityEngine;

public static class ComponentExtension_IsEnabled
{
    public static bool IsEnabled(this Component component)
    {
        if (component == null)
            return false;

        Behaviour b = component as Behaviour;
        return (b == null) || b.enabled;
    }
}
