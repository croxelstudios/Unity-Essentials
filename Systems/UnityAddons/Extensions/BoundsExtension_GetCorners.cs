using UnityEngine;

public static class BoundsExtension_GetCorners
{
    public static Vector3 GetCorner(this Bounds bounds, BoundsCorner corner)
    {
        return bounds.GetCorner(corner, Quaternion.identity);
    }

    public static Vector3 GetCorner(this Bounds bounds, BoundsCorner corner, Quaternion rotation)
    {
        Vector3 result = Vector3.zero;
        switch (corner)
        {
            case BoundsCorner.LTB:
                result = Vector3.Scale(bounds.extents, new Vector3(-1, 1, -1));
                break;
            case BoundsCorner.LTF:
                result = Vector3.Scale(bounds.extents, new Vector3(-1, 1, 1));
                break;
            case BoundsCorner.LBB:
                result = -bounds.extents;
                break;
            case BoundsCorner.LBF:
                result = Vector3.Scale(bounds.extents, new Vector3(-1, -1, 1));
                break;
            case BoundsCorner.RTB:
                result = Vector3.Scale(bounds.extents, new Vector3(1, 1, -1));
                break;
            case BoundsCorner.RTF:
                result = bounds.extents;
                break;
            case BoundsCorner.RBB:
                result = Vector3.Scale(bounds.extents, new Vector3(1, -1, -1));
                break;
            case BoundsCorner.RBF:
                result = Vector3.Scale(bounds.extents, new Vector3(1, -1, 1));
                break;
            default:
                result = Vector3.zero;
                break;
        }
        result = rotation * result;
        result += bounds.center;
        return result;
    }

    public static Vector3[] GetCorners(this Bounds bounds)
    {
        return bounds.GetCorners(Quaternion.identity);
    }

    public static Vector3[] GetCorners(this Bounds bounds, Quaternion rotation)
    {
        Vector3[] result = new Vector3[8];
        for (int i = 0; i < 8; i++)
            result[i] = bounds.GetCorner((BoundsCorner)i, rotation);
        return result;
    }
}

public enum BoundsCorner { LTB, LTF, LBB, LBF, RTB, RTF, RBB, RBF }

public enum BoundsEdge { LT, LB, RT, RB, LBa, LF, RBa, RF, TBa, TF, BBa, BF }
