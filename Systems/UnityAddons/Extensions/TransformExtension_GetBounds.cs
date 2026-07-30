using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension_GetBounds
{
    static Dictionary<Transform, Renderer> renderers;

    public static Bounds GetBounds(this Transform transform)
    {
        Renderer r = transform.GetRenderer();
        if (r != null) return r.bounds;
        else
        {
            NDCollider c = NDCollider.GetNDColliderFrom(transform);
            if (c != null) return c.bounds;
            else return new Bounds(transform.position, transform.lossyScale);
        }
    }

    public static Renderer GetRenderer(this Transform transform)
    {
        renderers = renderers.CreateIfNull_StaticPersistent();
        return renderers.GetComponent(transform);
    }
}
