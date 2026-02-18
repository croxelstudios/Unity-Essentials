using UnityEngine;

public static class CanvasExtension_PointerPosition
{
    public static Vector2 PointerPosition(this RectTransform rectTr)
    {
        Vector2 pos = Input.mousePosition.NormalizedScreenUV().Clamp01();
        pos -= Vector2.one * 0.5f;
        pos.Scale(rectTr.rect.size);
        return rectTr.TransformPoint(pos);
    }
}
