using UnityEngine;
using System.Collections.Generic;

public class NavMeshAgentOverride : MonoBehaviour
{
    [SerializeField]
    [NavMeshAgentTypeSelector]
    int agentType = 0;

    Dictionary<INavMeshAgentTypeContainer, int> oldValues;

    void OnEnable()
    {
        INavMeshAgentTypeContainer[] containers = GetComponentsInChildren<INavMeshAgentTypeContainer>();
        if (oldValues == null)
            oldValues = new Dictionary<INavMeshAgentTypeContainer, int> ();
        for (int i = 0; i < containers.Length; i++)
        {
            containers[i].OverrideNavMeshAgentType(agentType, out int prevAgentType);
            oldValues.Add(containers[i], prevAgentType);
        }
    }

    void OnDisable()
    {
        foreach (KeyValuePair<INavMeshAgentTypeContainer, int> pair in oldValues)
            pair.Key.OverrideNavMeshAgentType(pair.Value, out int prevAgentType);
        oldValues.Clear();
    }
}
