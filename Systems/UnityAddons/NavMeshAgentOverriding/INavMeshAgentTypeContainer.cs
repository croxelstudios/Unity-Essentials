using UnityEngine;

public interface INavMeshAgentTypeContainer
{
    public void OverrideNavMeshAgentType(int navMeshAgentType, out int prevAgentType);
}
