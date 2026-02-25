using System.Linq;
using UnityEngine;

public static class BoundsExtension_Draw
{
    public static void Draw(this Bounds bounds, Color color, BoundsEdge[] exclude = null)
    {
        Draw(bounds, color, Quaternion.identity, exclude);
    }

    public static void Draw(this Bounds bounds, Color color, Quaternion rotation, BoundsEdge[] exclude = null)
    {
        Vector3[] corners = bounds.GetCorners(rotation);

        if (!exclude.Contains(BoundsEdge.LT))
            Debug.DrawLine(corners[0], corners[1], color);
        if (!exclude.Contains(BoundsEdge.LB))
            Debug.DrawLine(corners[2], corners[3], color);
        if (!exclude.Contains(BoundsEdge.RT))
            Debug.DrawLine(corners[4], corners[3], color);
        if (!exclude.Contains(BoundsEdge.RB))
            Debug.DrawLine(corners[6], corners[7], color);

        if (!exclude.Contains(BoundsEdge.LBa))
            Debug.DrawLine(corners[0], corners[2], color);
        if (!exclude.Contains(BoundsEdge.LF))
            Debug.DrawLine(corners[1], corners[3], color);
        if (!exclude.Contains(BoundsEdge.RBa))
            Debug.DrawLine(corners[4], corners[6], color);
        if (!exclude.Contains(BoundsEdge.RF))
            Debug.DrawLine(corners[5], corners[7], color);

        if (!exclude.Contains(BoundsEdge.TBa))
            Debug.DrawLine(corners[0], corners[4], color);
        if (!exclude.Contains(BoundsEdge.TF))
            Debug.DrawLine(corners[1], corners[5], color);
        if (!exclude.Contains(BoundsEdge.BBa))
            Debug.DrawLine(corners[2], corners[6], color);
        if (!exclude.Contains(BoundsEdge.BF))
            Debug.DrawLine(corners[3], corners[7], color);
    }

    public static void Draw(this Bounds bounds, Color color, float duration, BoundsEdge[] exclude = null)
    {
        Draw(bounds, color, Quaternion.identity, duration, exclude);
    }

    public static void Draw(this Bounds bounds, Color color, Quaternion rotation, float duration, BoundsEdge[] exclude = null)
    {
        Vector3[] corners = bounds.GetCorners(rotation);

        if (!exclude.Contains(BoundsEdge.LT))
            Debug.DrawLine(corners[0], corners[1], color, duration);
        if (!exclude.Contains(BoundsEdge.LB))
            Debug.DrawLine(corners[2], corners[3], color, duration);
        if (!exclude.Contains(BoundsEdge.RT))
            Debug.DrawLine(corners[4], corners[3], color, duration);
        if (!exclude.Contains(BoundsEdge.RB))
            Debug.DrawLine(corners[6], corners[7], color, duration);

        if (!exclude.Contains(BoundsEdge.LBa))
            Debug.DrawLine(corners[0], corners[2], color, duration);
        if (!exclude.Contains(BoundsEdge.LF))
            Debug.DrawLine(corners[1], corners[3], color, duration);
        if (!exclude.Contains(BoundsEdge.RBa))
            Debug.DrawLine(corners[4], corners[6], color, duration);
        if (!exclude.Contains(BoundsEdge.RF))
            Debug.DrawLine(corners[5], corners[7], color, duration);

        if (!exclude.Contains(BoundsEdge.TBa))
            Debug.DrawLine(corners[0], corners[4], color, duration);
        if (!exclude.Contains(BoundsEdge.TF))
            Debug.DrawLine(corners[1], corners[5], color, duration);
        if (!exclude.Contains(BoundsEdge.BBa))
            Debug.DrawLine(corners[2], corners[6], color, duration);
        if (!exclude.Contains(BoundsEdge.BF))
            Debug.DrawLine(corners[3], corners[7], color, duration);
    }
}
