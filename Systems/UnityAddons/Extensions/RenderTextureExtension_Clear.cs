using UnityEngine;

public static class RenderTextureExtension_Clear
{
    public static void Clear(this RenderTexture texture)
    {
        texture.Clear(Color.clear);
    }

    public static void Clear(this RenderTexture texture, Color toColor)
    {
        RenderTexture.active = texture;
        GL.Clear(true, true, toColor);
        RenderTexture.active = null;
    }
}
