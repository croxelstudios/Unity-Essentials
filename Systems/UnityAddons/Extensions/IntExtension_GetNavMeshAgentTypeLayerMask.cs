using Unity.AI.Navigation;
using UnityEngine;

public static class IntExtension_GetNavMeshAgentTypeLayerMask
{
    static NavMeshSurface[] navMeshSurfaces;

    public static LayerMask GetNavMeshAgentTypeLayerMask(this int navMeshAgentType)
    {
        if (navMeshSurfaces == null)
            navMeshSurfaces = GameObject.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);

        if (navMeshSurfaces != null)
        {
            LayerMask result = 0;
            for (int i = 0; i < navMeshSurfaces.Length; i++)
                if (navMeshSurfaces[i].agentTypeID == navMeshAgentType)
                    result = result.Combine(navMeshSurfaces[i].layerMask);
            return result;
        }
        else return 0;
    }
}
