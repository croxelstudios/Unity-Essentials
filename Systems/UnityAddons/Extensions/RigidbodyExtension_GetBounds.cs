using System.Collections.Generic;
using UnityEngine;

public static class RigidbodyExtension_GetBounds
{
    public static Bounds GetBounds(this Rigidbody2D rigid2D)
    {
        List<Collider2D> colliders = new List<Collider2D>();
        rigid2D.GetAttachedColliders(colliders);
        Bounds bounds;
        if (colliders.Count > 0)
        {
            bounds = colliders[0].bounds;
            if (colliders.Count > 1)
                for (int i = 1; i < colliders.Count; i++)
                    bounds.Encapsulate(colliders[i].bounds);
        }
        else bounds = new Bounds(Vector3.zero, Vector3.zero);
        return bounds;
    }

    public static Bounds GetBounds(this Rigidbody rigid)
    {
        Collider[] colliders = rigid.GetComponentsInChildren<Collider>();
        bool registered = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        if (colliders.Length > 0)
            for (int i = 0; i < colliders.Length; i++)
                if (!colliders[i].isTrigger)
                {
                    if (!registered)
                    {
                        bounds = colliders[i].bounds;
                        registered = true;
                    }
                    else bounds.Encapsulate(colliders[i].bounds);
                }
        return bounds;
    }
}
