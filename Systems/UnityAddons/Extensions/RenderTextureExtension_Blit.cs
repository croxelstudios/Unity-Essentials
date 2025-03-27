using UnityEngine;

public static class RenderTextureExtension_Blit
{
    public static void Blit(this Material material, ref RenderTexture rt,
        string name, Vector2Int textureResolution, int depth = 32,
        RenderTextureFormat format = RenderTextureFormat.ARGB32)
    {
        if (rt == null)
        {
            rt = new RenderTexture(textureResolution.x, textureResolution.y, depth, format);
            rt.Create();
        }
        rt.name = name;
        Graphics.Blit(null, rt, material, 0);
    }

    public static void Blit(this Material material, ref RenderTexture rt,
        string name = "", int textureResolution = 360, int depth = 32,
        RenderTextureFormat format = RenderTextureFormat.ARGB32)
    {
        Blit(material, ref rt, name,
            new Vector2Int(textureResolution, textureResolution), depth, format);
    }
}
