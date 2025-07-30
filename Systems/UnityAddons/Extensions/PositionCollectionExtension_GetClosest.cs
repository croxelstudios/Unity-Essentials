using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PositionCollectionExtension_GetClosest
{
    public static T GetClosest<T>(this IEnumerable<T> collection, Vector3 position) where T : Object
    {
        T closestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        foreach (T potentialTarget in collection)
        {
            Transform tr = potentialTarget.GetTransform();
            Vector3 directionToTarget = tr.position - position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closestTarget = potentialTarget;
            }
        }

        return closestTarget;
    }

    public static T GetClosest<T>(this IEnumerable<T> collection, Transform target) where T : Object
    {
        return collection.GetClosest(target.position);
    }

    public static Vector3 GetClosest(this IEnumerable<Vector3> collection, Vector3 position)
    {
        Vector3 closestTarget = collection.First();
        float closestDistanceSqr = Mathf.Infinity;
        foreach (Vector3 potentialTarget in collection)
        {
            Vector3 directionToTarget = potentialTarget - position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closestTarget = potentialTarget;
            }
        }

        return closestTarget;
    }

    public static Vector3 GetClosest(this IEnumerable<Vector3> collection, Transform target)
    {
        return collection.GetClosest(target.position);
    }

    public static Vector2 GetClosest(this IEnumerable<Vector2> collection, Vector2 position)
    {
        Vector2 closestTarget = collection.First();
        float closestDistanceSqr = Mathf.Infinity;
        foreach (Vector2 potentialTarget in collection)
        {
            Vector2 directionToTarget = potentialTarget - position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closestTarget = potentialTarget;
            }
        }

        return closestTarget;
    }

    public static Vector2 GetClosest(this IEnumerable<Vector2> collection, Transform target)
    {
        return collection.GetClosest(target.position);
    }

    public static Vector4 GetClosest(this IEnumerable<Vector4> collection, Vector4 position)
    {
        Vector4 closestTarget = collection.First();
        float closestDistanceSqr = Mathf.Infinity;
        foreach (Vector4 potentialTarget in collection)
        {
            Vector4 directionToTarget = potentialTarget - position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closestTarget = potentialTarget;
            }
        }

        return closestTarget;
    }
}
