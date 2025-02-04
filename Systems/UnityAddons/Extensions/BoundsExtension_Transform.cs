using UnityEngine;

public static class BoundsExtension_Transform
{
    public static Bounds Transform(this Bounds bounds, Transform transform)
    {
        return bounds.Transform(transform.position, transform.lossyScale, transform.rotation);
    }

    public static Bounds Transform(this Bounds bounds, TransformData transform)
    {
        return bounds.Transform(transform.position, transform.lossyScale, transform.rotation);
    }

    public static Bounds Transform(this Bounds bounds,
        Vector3 position, Vector3 scale, Quaternion rotation)
    {
        //TO DO: The transformation applied is different to the
        //one applied by unity to the renderer when scaling a parent
        //of a rotated object, but the one used by unity results in
        //completely broken bounds, so this might be better.
        Bounds nBounds = new Bounds(bounds.center + position, bounds.size);

        Vector3 exRTF = nBounds.extents;
        Vector3 exRTB = Vector3.Scale(nBounds.extents, new Vector3(1, 1, -1));
        Vector3 exRBF = Vector3.Scale(nBounds.extents, new Vector3(1, -1, 1));
        Vector3 exLTF = Vector3.Scale(nBounds.extents, new Vector3(-1, 1, 1));

        Vector3 cornerRTF = nBounds.center + (rotation *
            Vector3.Scale(exRTF, scale));
        Vector3 cornerRTB = nBounds.center + (rotation *
            Vector3.Scale(exRTB, scale));
        Vector3 cornerRBF = nBounds.center + (rotation *
            Vector3.Scale(exRBF, scale));
        Vector3 cornerLTF = nBounds.center + (rotation *
            Vector3.Scale(exLTF, scale));
        Vector3 cornerLBB = -cornerRTF;
        Vector3 cornerLBF = -cornerRTB;
        Vector3 cornerLTB = -cornerRBF;
        Vector3 cornerRBB = -cornerLTF;

        nBounds.Encapsulate(cornerRTF);
        nBounds.Encapsulate(cornerRTB);
        nBounds.Encapsulate(cornerRBF);
        nBounds.Encapsulate(cornerLTF);
        nBounds.Encapsulate(cornerLBB);
        nBounds.Encapsulate(cornerLBF);
        nBounds.Encapsulate(cornerLTB);
        nBounds.Encapsulate(cornerRBB);

        return nBounds;
    }
}
