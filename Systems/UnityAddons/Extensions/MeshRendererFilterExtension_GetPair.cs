using System.Collections.Generic;
using UnityEngine;

public static class MeshRendererFilterExtension_GetPair
{
    static Dictionary<MeshFilter, MeshRenderer> renderers;
    static Dictionary<MeshRenderer, MeshFilter> filters;

    public static MeshRenderer GetRenderer(this MeshFilter filter)
    {
        renderers = renderers.CreateIfNull_StaticPersistent();
        filters = filters.CreateIfNull_StaticPersistent();

        MeshRenderer rend = renderers.GetComponent(filter);
        filters.Set(rend, filter);
        return rend;
    }

    public static MeshFilter GetFilter(this MeshRenderer renderer)
    {
        renderers = renderers.CreateIfNull_StaticPersistent();
        filters = filters.CreateIfNull_StaticPersistent();

        MeshFilter filter = filters.GetComponent(renderer);
        renderers.Set(filter, renderer);
        return filter;
    }

    public static MeshFilter GetFilter(this Renderer renderer)
    {
        return (renderer is MeshRenderer mRend) ? mRend.GetFilter() : null;
    }
}
