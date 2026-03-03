using UnityEngine;

public static class BoundsExtension_BoundingSphere
{
    public static BoundingSphere BoundingSphere(this Bounds bounds)
    {
        return new BoundingSphere(bounds.center, bounds.extents.magnitude);
    }
}
