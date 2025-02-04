using UnityEngine;

public static class BoundsExtension_DrawBounds
{
    public static void DrawBounds(this Bounds bounds, Color color)
    {
        Vector3 cornerRTF = bounds.center + bounds.extents;
        Vector3 cornerRTB = bounds.center + 
            Vector3.Scale(bounds.extents, new Vector3(1, 1, -1));
        Vector3 cornerRBF = bounds.center + 
            Vector3.Scale(bounds.extents, new Vector3(1, -1, 1));
        Vector3 cornerLTF = bounds.center + 
            Vector3.Scale(bounds.extents, new Vector3(-1, 1, 1));
        Vector3 cornerLBB = -cornerRTF;
        Vector3 cornerLBF = -cornerRTB;
        Vector3 cornerLTB = -cornerRBF;
        Vector3 cornerRBB = -cornerLTF;

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
