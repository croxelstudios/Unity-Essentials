using UnityEngine;

public static class BoundsExtension_DrawBounds
{
    public static void DrawBounds(this Bounds bounds, Color color)
    {
        DrawBounds(bounds, color, Quaternion.identity);
    }

    public static void DrawBounds(this Bounds bounds, Color color, Quaternion rotation)
    {
        Vector3 cornerRTF = bounds.extents;
        Vector3 cornerRTB = Vector3.Scale(bounds.extents, new Vector3(1, 1, -1));
        Vector3 cornerRBF = Vector3.Scale(bounds.extents, new Vector3(1, -1, 1));
        Vector3 cornerLTF = Vector3.Scale(bounds.extents, new Vector3(-1, 1, 1));
        Vector3 cornerLBB = -cornerRTF;
        Vector3 cornerLBF = -cornerRTB;
        Vector3 cornerLTB = -cornerRBF;
        Vector3 cornerRBB = -cornerLTF;

        cornerLBB = rotation * cornerLBB;
        cornerLBF = rotation * cornerLBF;
        cornerLTB = rotation * cornerLTB;
        cornerRBB = rotation * cornerRBB;
        cornerRTF = rotation * cornerRTF;
        cornerRTB = rotation * cornerRTB;
        cornerRBF = rotation * cornerRBF;
        cornerLTF = rotation * cornerLTF;

        cornerLBB += bounds.center;
        cornerLBF += bounds.center;
        cornerLTB += bounds.center;
        cornerRBB += bounds.center;
        cornerRTF += bounds.center;
        cornerRTB += bounds.center;
        cornerRBF += bounds.center;
        cornerLTF += bounds.center;

        Debug.DrawLine(cornerRTF, cornerRTB, color);
        Debug.DrawLine(cornerRTF, cornerRBF, color);
        Debug.DrawLine(cornerRTF, cornerLTF, color);
        Debug.DrawLine(cornerLBB, cornerLBF, color);
        Debug.DrawLine(cornerLBB, cornerLTB, color);
        Debug.DrawLine(cornerLBB, cornerRBB, color);

        Debug.DrawLine(cornerRTB, cornerLTB, color);
        Debug.DrawLine(cornerRTB, cornerRBB, color);
        Debug.DrawLine(cornerRBF, cornerLBF, color);
        Debug.DrawLine(cornerRBF, cornerRBB, color);
        Debug.DrawLine(cornerLTF, cornerLBF, color);
        Debug.DrawLine(cornerLTF, cornerLTB, color);
    }
}
