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
        //The transformation applied is different to the
        //one applied by unity to the renderer when scaling a parent
        //of a rotated object, but the one used by unity results in
        //completely broken bounds, so this might be better.
        Vector3 exRTF = bounds.GetCorner(BoundsCorner.RTF);
        Vector3 exRTB = bounds.GetCorner(BoundsCorner.RTB);
        Vector3 exRBF = bounds.GetCorner(BoundsCorner.RBF);
        Vector3 exLTF = bounds.GetCorner(BoundsCorner.LTF);
        Vector3 exLBB = bounds.GetCorner(BoundsCorner.LBB);
        Vector3 exLBF = bounds.GetCorner(BoundsCorner.LBF);
        Vector3 exLTB = bounds.GetCorner(BoundsCorner.LTB);
        Vector3 exRBB = bounds.GetCorner(BoundsCorner.RBB);

        Vector3 cornerRTF = rotation * Vector3.Scale(exRTF, scale);
        Vector3 cornerRTB = rotation * Vector3.Scale(exRTB, scale);
        Vector3 cornerRBF = rotation * Vector3.Scale(exRBF, scale);
        Vector3 cornerLTF = rotation * Vector3.Scale(exLTF, scale);
        Vector3 cornerLBB = rotation * Vector3.Scale(exLBB, scale);
        Vector3 cornerLBF = rotation * Vector3.Scale(exLBF, scale);
        Vector3 cornerLTB = rotation * Vector3.Scale(exLTB, scale);
        Vector3 cornerRBB = rotation * Vector3.Scale(exRBB, scale);

        cornerLBB += position;
        cornerLBF += position;
        cornerLTB += position;
        cornerRBB += position;
        cornerRTF += position;
        cornerRTB += position;
        cornerRBF += position;
        cornerLTF += position;

        Bounds nBounds = new Bounds(cornerRTF, Vector3.zero);
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
