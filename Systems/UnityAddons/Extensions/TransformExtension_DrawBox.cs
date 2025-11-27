using UnityEngine;

public static class TransformExtension_DrawBox
{
    public static void DrawBox(this Transform tr, Color color)
    {
        Vector3 center = tr.position;
        Vector3 size = tr.lossyScale;
        Bounds bounds = new Bounds(center, size);
        bounds.Draw(color, tr.rotation);
    }
}
