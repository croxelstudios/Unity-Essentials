using System.Collections.Generic;
using UnityEngine;

public static class RigidbodyExtension_GetBounds
{
    static List<Collider2D> col2;

    public static Bounds GetBounds(this Rigidbody2D rigid2D)
    {
        rigid2D.GetAttachedColliders(col2);
        Bounds bounds;
        if (col2.Count > 0)
        {
            bounds = col2[0].bounds;
            if (col2.Count > 1)
                for (int i = 1; i < col2.Count; i++)
                    bounds.Encapsulate(col2[i].bounds);
        }
        else bounds = new Bounds(Vector3.zero, Vector3.zero);
        col2.Clear();
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
