using Mono.CSharp;
using UnityEngine;

public static class ComponentExtension_GetTransform
{
    public static Transform GetTransform(this Component comp)
    {
        Transform tr = comp as Transform;
        if (tr != null) return tr;
        else
        {
            Behaviour behaviour = comp as Behaviour;
            if (behaviour != null)
                return behaviour.transform;
            else return null;
        }
    }
}
