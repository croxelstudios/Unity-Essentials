#if UNITY_EDITOR
using System;
using UnityEngine;

public static class TypeExtension_DisallowsMultiple
{
    public static bool DisallowsMultiple(this Type type)
    {
        switch (type)
        {
            case Type t when t.IsOrInheritsFrom(typeof(GameObject)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(Transform)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(Rigidbody)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(ArticulationBody)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(Rigidbody2D)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(Animator)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(Animation)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(AudioListener)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(Camera)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(LightProbeProxyVolume)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(ReflectionProbe)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(Terrain)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(Canvas)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(MeshFilter)):
                return true;
            case Type t when t.IsOrInheritsFrom(typeof(MeshRenderer)):
                return true;
            case Type t when t.IsOrInheritsFrom(Type.GetType("NavMeshAgent")):
                return true;
            case Type t when t.IsOrInheritsFrom(Type.GetType("NavMeshObstacle")):
                return true;
            default:
                return type.IsDefined(typeof(DisallowMultipleComponent), true);
        }
    }
}
#endif
