using UnityEngine;

public static class LayerMaskExtension_ContainsLayer
{
    public static bool ContainsLayer(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}
