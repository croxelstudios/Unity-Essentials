using UnityEngine;
using System.Collections.Generic;

public static class IEnumerableExtension_GetLayers
{
    public static LayerMask GetLayers<T>(this IEnumerable<T> collection) where T : Component
    {
        LayerMask layerMask = 0;
        foreach (Component item in collection)
            if (item != null)
                layerMask = layerMask.AddLayer(item.gameObject.layer);
        return layerMask;
    }

    public static LayerMask GetLayers(this IEnumerable<GameObject> collection)
    {
        LayerMask layerMask = 0;
        foreach (GameObject item in collection)
            if (item != null)
                layerMask = layerMask.AddLayer(item.layer);
        return layerMask;
    }
}
