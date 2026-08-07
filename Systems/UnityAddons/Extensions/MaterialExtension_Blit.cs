using UnityEngine;

public static class MaterialExtension_Blit
{
    public static void Blit(this Material material, ref RenderTexture rt,
        string name, Vector2Int textureResolution, int depth = 32,
        RenderTextureFormat format = RenderTextureFormat.ARGB32)
    {
        if (rt == null)
            rt = new RenderTexture(textureResolution.x, textureResolution.y, depth, format);
        else if (((rt.width != textureResolution.x) || (rt.height != textureResolution.y) ||
            (rt.depth != depth) || (rt.format != format)))
        {
            rt.Release();
            rt.width = textureResolution.x;
            rt.height = textureResolution.y;
            rt.depth = depth;
            rt.format = format;
        }

        if (!rt.IsCreated())
            rt.Create();

        rt.name = name;
        //material.GLRenderToRT(rt);
        Texture tex = material.HasProperty("_MainTex") ?
            material.GetTexture("_MainTex") : null;
        Graphics.Blit(tex, rt, material, 0);
    }

    public static void Blit(this Material material, ref RenderTexture rt,
        string name = "", int textureResolution = 360, int depth = 32,
        RenderTextureFormat format = RenderTextureFormat.ARGB32)
    {
        Blit(material, ref rt, name,
            new Vector2Int(textureResolution, textureResolution), depth, format);
    }

    public static void GLRenderToRT(this Material material, RenderTexture rt, bool cleanFirst)
    {
        material.GLRenderToRT(rt, 0, cleanFirst);
    }

    public static void GLRenderToRT(this Material material, RenderTexture rt, int pass = 0)
    {
        material.GLRenderToRT(rt, pass, false);
    }

    public static void GLRenderToRT(this Material material, RenderTexture rt, int pass, bool cleanFirst)
    {
        if ((material == null) || (rt == null))
            return;

        RenderTexture previousTarget = RenderTexture.active;
        bool previousSrgbWrite = GL.sRGBWrite;

        Graphics.SetRenderTarget(rt);
        GL.Viewport(new Rect(0, 0, rt.width, rt.height));

        if (cleanFirst) GL.Clear(true, true, Color.clear);

        GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;

        GL.PushMatrix();
        GL.LoadOrtho();

        if (material.SetPass(pass))
        {
            GL.Begin(GL.QUADS);

            GL.TexCoord2(0f, 0f); GL.Vertex3(0f, 0f, 0f);
            GL.TexCoord2(1f, 0f); GL.Vertex3(1f, 0f, 0f);
            GL.TexCoord2(1f, 1f); GL.Vertex3(1f, 1f, 0f);
            GL.TexCoord2(0f, 1f); GL.Vertex3(0f, 1f, 0f);

            GL.End();
        }

        GL.PopMatrix();

        GL.sRGBWrite = previousSrgbWrite;
        Graphics.SetRenderTarget(previousTarget);
    }
}
