using UnityEngine;

public static class LayerMaskExtension_Combine
{
    public static LayerMask SubtractLayer(this LayerMask layerMask, int layer)
    {
        return layerMask & ~(1 << layer);
    }

    public static LayerMask AddLayer(this LayerMask layerMask, int layer)
    {
        return layerMask | (1 << layer);
    }

    public static LayerMask Combine(this LayerMask layerMask, LayerMask other)
    {
        return layerMask | other;
    }

    public static LayerMask SubtractLayerMask(this LayerMask layerMask, LayerMask other)
    {
        return layerMask & ~other;
    }
}
