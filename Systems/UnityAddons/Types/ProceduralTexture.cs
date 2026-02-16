using System;
using UnityEngine;

[Serializable]
public struct ProceduralTexture
{
    //TO DO: Support material array to stack effects
    [SerializeField]
    Material textureMaterial;
    public readonly Material material => textureMaterial;
    [SerializeField]
    [Min(1)]
    int textureResolution;
    [SerializeField]
    TextureWrapMode wrapMode;
    [SerializeField]
    FilterMode filterMode;
    [SerializeField]
    int depth;
    [SerializeField]
    RenderTextureFormat format;

    [HideInInspector]
    public RenderTexture rt;

    public ProceduralTexture(Material textureMaterial)
    {
        this.textureMaterial = textureMaterial;
        textureResolution = 360;
        wrapMode = TextureWrapMode.Clamp;
        filterMode = FilterMode.Point;
        depth = 0;
        format = RenderTextureFormat.ARGB32;
        rt = null;
    }

    public ProceduralTexture(Material textureMaterial, int textureResolution, TextureWrapMode wrapMode, FilterMode filterMode,
        int depth = 0, RenderTextureFormat format = RenderTextureFormat.ARGB32)
    {
        this.textureMaterial = textureMaterial;
        this.textureResolution = textureResolution;
        this.wrapMode = wrapMode;
        this.filterMode = filterMode;
        this.depth = depth;
        this.format = format;
        rt = null;
    }

    public bool IsValid()
    {
        return textureMaterial != null;
    }

    public void Update(string name = "ProceduralTexture")
    {
        textureMaterial.Blit(ref rt, name, textureResolution, depth, format);
        rt.wrapMode = wrapMode;
        rt.filterMode = filterMode;
    }

    public void Release()
    {
        if (rt != null)
        {
            rt.Release();
            rt = null;
        }
    }
}
