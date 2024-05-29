using UnityEngine;

public static class BoundsExtension_DrawBounds
{
    public static void DrawBounds(this Bounds bounds, Color color)
    {
        Vector3 upperLeft = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y);
        Vector3 upperRight = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y);
        Vector3 bottomLeft = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y);
        Vector3 bottonRight = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y);
        Debug.DrawLine(upperLeft, upperRight, color);
        Debug.DrawLine(upperRight, bottonRight, color);
        Debug.DrawLine(bottomLeft, bottonRight, color);
        Debug.DrawLine(upperLeft, bottomLeft, color);
    }
}
