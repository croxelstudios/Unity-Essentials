using UnityEngine;

public static class ParticleSystem_GetSimulationSpace
{
    public static Transform GetSimulationSpace(this ParticleSystem system)
    {
        switch (system.main.simulationSpace)
        {
            case ParticleSystemSimulationSpace.World:
                return null;
            case ParticleSystemSimulationSpace.Custom:
                return system.main.customSimulationSpace;
            default: return system.transform;
        }
    }
}
